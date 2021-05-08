using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Raven.Client.Documents;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Infrastructure.StartupTasks;

namespace Raven.Yabt.WebApi.Infrastructure
{
	/// <summary>
	///		A start-up task to update DB indexes.
	///		Need to be executed in DEV mode only (to simplify development)
	/// </summary>
	/// <remarks>
	///		Based on https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-4-using-health-checks/
	/// </remarks>
	public class DbStartupTask : BackgroundService, IStartupTask
	{
		private readonly StartupTaskContext _startupTaskContext;
		private readonly IDocumentStore _store;

		public DbStartupTask(StartupTaskContext startupTaskContext, IDocumentStore store)
		{
			_startupTaskContext = startupTaskContext;
			_store = store;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _store.CreateUpdateIndexes();

			_startupTaskContext.MarkTaskAsComplete();
		}
	}
}
