using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.TicketImporter.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register the document store as single instance, initializing it on first use
	/// </summary>
	public static IServiceCollection AddAndConfigureDatabaseSessionForImport(this IServiceCollection services)
	{
		return services.AddAndConfigureDatabase(store => store.Conventions.MaxNumberOfRequestsPerSession = 40_000)
		               .AddSingleton(c =>
		               {
			               var session = c.GetRequiredService<IDocumentStore>().OpenAsyncSession();
			               session.Advanced.WaitForIndexesAfterSaveChanges();  // Wait on each change to avoid adding WaitForIndexing() in each test
			               return session;
		               })
		               .AddAndConfigureDatabaseTenantedSession(x => x.GetRequiredService<ICurrentTenantResolver>().GetCurrentTenantId);
	}
}