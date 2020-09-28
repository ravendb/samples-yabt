using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Exceptions.Database;
using Raven.Migrations;

namespace Raven.Yabt.Database.Migration
{
	/// <summary>
	///		Migrate the DB schema
	/// </summary>
	public class MigrationService : IHostedService
	{
		private readonly MigrationRunner _runner;
		private readonly IDocumentStore _store;

		public MigrationService(MigrationRunner runner, IDocumentStore store)
		{
			_runner = runner;
			_store = store;
			if (string.IsNullOrEmpty(_store.Database))
				throw DatabaseDoesNotExistException.CreateWithMessage(_store.Database, "No database specified");
		}

		/// <inheritdoc/>
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_runner.Run();
			if (cancellationToken.IsCancellationRequested)
				return;

			await IndexCreation.CreateIndexesAsync(typeof(SetupDocumentStore).Assembly, _store, null, _store.Database, cancellationToken);

			var indexErrors = await GetIndexErrors(maxWaitingForStaleIndexes: 30, cancellationToken);

			if (!string.IsNullOrEmpty(indexErrors))
				throw new DatabaseDisabledException(indexErrors);
		}

		/// <inheritdoc/>
		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

		/// <summary>
		///		Check for errors with the DB indexes
		/// </summary>
		/// <param name="maxWaitingForStaleIndexes"> Max waiting interval for checking stale indexes </param>
		/// <param name="cancellationToken"> The task cancelation token </param>
		/// <returns> Error message if fail, otherwise - NULL </returns>
		private async Task<string?> GetIndexErrors(int maxWaitingForStaleIndexes, CancellationToken cancellationToken)
		{
			// Check for errors in the indexes
			async Task<string?> checkIndexErrors()
			{
				var indexErrors = await _store.Maintenance.SendAsync(new GetIndexErrorsOperation());
				return indexErrors?.Any(x => x.Errors.Length > 0) == true
					? string.Format(
						"There are indexes with errors after migration: {0}",
						indexErrors.Where(x => x.Errors.Length > 0)
								.Select(x => x.Name)
								.Aggregate((i, j) => $"'{i}','{j}'"))
					: null;
			}

			var indexErrorMsg = await checkIndexErrors();
			if (!string.IsNullOrEmpty(indexErrorMsg))
				return indexErrorMsg;

			// Wait till indexes stop being stale			
			bool staleIndexes;
			var date = DateTime.Now;

			do
			{
				if (cancellationToken.IsCancellationRequested)
					return "Task cancelled";

				var dbStats = await _store.Maintenance.SendAsync(new GetDetailedStatisticsOperation());
				staleIndexes = dbStats.Indexes.Any(x => x.IsStale);

				if (staleIndexes)
				{
					if ((DateTime.Now - date).TotalSeconds > maxWaitingForStaleIndexes)
					{
						return $"Timeout: After {maxWaitingForStaleIndexes} secs indexes are still stale...";
					}

					indexErrorMsg = await checkIndexErrors();

					if (!string.IsNullOrEmpty(indexErrorMsg))
						return indexErrorMsg;

					await Task.Delay(1000);
				}
			} while (staleIndexes);

			return null;
		}
	}
}