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
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Migration.Configuration;

namespace Raven.Yabt.Database.Migration;

/// <summary>
///		A service to migrate the DB schema on the start of the app 
/// </summary>
public class MigrationService : IHostedService
{
	private readonly MigrationRunner _runner;
	private readonly IDocumentStore _store;
	private readonly AppSettings _settings;
	private readonly IHostApplicationLifetime _applicationLifetime;

	public MigrationService(MigrationRunner runner, IDocumentStore store, AppSettings settings, IHostApplicationLifetime applicationLifetime)
	{
		_runner = runner;
		_store = store;
		_settings = settings;
		_applicationLifetime = applicationLifetime;
		if (string.IsNullOrEmpty(_store.Database))
			throw DatabaseDoesNotExistException.CreateWithMessage(_store.Database, "No database specified");
	}

	/// <inheritdoc/>
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		// Run all the migrations
		_runner.Run();
		if (cancellationToken.IsCancellationRequested)
			return;

		// Update indexes
		await IndexCreation.CreateIndexesAsync(typeof(SetupDocumentStore).Assembly, _store, null, _store.Database, cancellationToken);

		// Check for errors in indexes
		var indexErrors = await GetIndexErrors(maxWaitingForStaleIndexes: _settings.MaxWaitingPeriodForRebuildingStaleIndexes, cancellationToken);

		if (!string.IsNullOrEmpty(indexErrors))
			throw new DatabaseDisabledException(indexErrors);
			
		// Stop the application rather than waiting for Ctrl+C from the user
		_applicationLifetime.StopApplication();
	}

	/// <inheritdoc/>
	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	/// <summary>
	///		Check for errors with the DB indexes
	/// </summary>
	/// <param name="maxWaitingForStaleIndexes"> Max waiting interval for rebuilding stale indexes. 0 - infinite wait </param>
	/// <param name="cancellationToken"> The cancellation token </param>
	/// <returns> Error message if fail, otherwise - NULL </returns>
	private async Task<string?> GetIndexErrors(int maxWaitingForStaleIndexes, CancellationToken cancellationToken)
	{
		// Check for errors in the indexes
		async Task<string?> CheckIndexErrors()
		{
			var indexErrors = await _store.Maintenance.SendAsync(new GetIndexErrorsOperation(), cancellationToken);
			return indexErrors?.Any(x => x.Errors.Length > 0) == true
				? string.Format(
					"There are indexes with errors after migration: {0}",
					indexErrors.Where(x => x.Errors.Length > 0)
					           .Select(x => x.Name)
					           .Aggregate((i, j) => $"'{i}','{j}'"))
				: null;
		}

		var indexErrorMsg = await CheckIndexErrors();
		if (!string.IsNullOrEmpty(indexErrorMsg))
			return indexErrorMsg;

		// Wait till indexes stop being stale			
		bool staleIndexes;
		var date = DateTime.Now;

		do
		{
			if (cancellationToken.IsCancellationRequested)
				return "Task cancelled";

			var dbStats = await _store.Maintenance.SendAsync(new GetDetailedStatisticsOperation(), cancellationToken);
			staleIndexes = dbStats.Indexes.Any(x => x.IsStale);

			if (staleIndexes)
			{
				if (0 < maxWaitingForStaleIndexes && maxWaitingForStaleIndexes < (DateTime.Now - date).TotalSeconds)
				{
					return $"Timeout: After {maxWaitingForStaleIndexes} secs indexes are still stale...";
				}

				indexErrorMsg = await CheckIndexErrors();

				if (!string.IsNullOrEmpty(indexErrorMsg))
					return indexErrorMsg;

				await Task.Delay(1000, cancellationToken);
			}
		} while (staleIndexes);

		return null;
	}
}