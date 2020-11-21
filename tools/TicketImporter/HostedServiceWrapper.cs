using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Yabt.Database;
using Raven.Yabt.TicketImporter.Services;

namespace Raven.Yabt.TicketImporter
{
	/// <summary>
	///		A wrapper for singleton IHostedService to use scoped dependencies
	/// </summary>
	/// <remarks>
	///		Based on https://stackoverflow.com/a/55984032/968003
	/// </remarks>
	internal class HostedServiceWrapper : IHostedService
	{
		private readonly IServiceScopeFactory _serviceFactory;

		public HostedServiceWrapper(IServiceScopeFactory serviceFactory)
		{
			_serviceFactory = serviceFactory;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceFactory.CreateScope();
			
			// Make sure that indexes are in place
			var dbStore = scope.ServiceProvider.GetService<IDocumentStore>()!;
			await IndexCreation.CreateIndexesAsync(typeof(SetupDocumentStore).Assembly, dbStore, null, dbStore.Database, cancellationToken);
			
			// Kick off the import process
			var worker = scope.ServiceProvider.GetService<TicketImportService>()!;
			await worker.Run(cancellationToken);
		}

		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
