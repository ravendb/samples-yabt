using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Yabt.Database.Infrastructure;

namespace Raven.Yabt.TicketImporter;

/// <summary>
///		A wrapper for singleton IHostedService to use scoped dependencies and run the <see cref="MainWorker"/>
/// </summary>
internal class HostedServiceWrapper : IHostedService
{
	private readonly IServiceScopeFactory _serviceFactory;
	private readonly IDocumentStore _store;
	private readonly IHostApplicationLifetime _applicationLifetime;

	public HostedServiceWrapper(IServiceScopeFactory serviceFactory, IDocumentStore store, IHostApplicationLifetime applicationLifetime)
	{
		_serviceFactory = serviceFactory;
		_applicationLifetime = applicationLifetime;
		_store = store;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		// Make sure that indexes are in place
		await IndexCreation.CreateIndexesAsync(typeof(SetupDocumentStore).Assembly, _store, null, _store.Database, cancellationToken);
			
		// Kick off the import process
		// Creating a scope to consume scoped dependencies (mostly coming from 'Domain' proj) in the singleton instance. See more at https://stackoverflow.com/a/55984032/968003
		using (var scope = _serviceFactory.CreateScope())
		{
			var worker = scope.ServiceProvider.GetRequiredService<MainWorker>();
			await worker.Run(cancellationToken);
		}

		// Stop the application rather than waiting for Ctrl+C from the user
		_applicationLifetime.StopApplication();
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}