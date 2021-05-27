using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Database;
using Raven.Yabt.Database.Common.Configuration;

namespace Raven.Yabt.TicketImporter.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register the document store as single instance, initializing it on first use
		/// </summary>
		public static IServiceCollection AddAndConfigureDatabaseForImport(this IServiceCollection services)
		{
			services.AddSingleton(x =>
				{
					var config = x.GetService<DatabaseSettings>();
					return SetupDocumentStore.GetDocumentStore(
						config!,
						true, 
						(store) => store.Conventions.MaxNumberOfRequestsPerSession = 20000, 
						true);
				});
			services.AddScoped(c =>
				{
					var session = c.GetService<IDocumentStore>()!.OpenAsyncSession();
						session.Advanced.WaitForIndexesAfterSaveChanges();  // Wait on each change to avoid adding WaitForIndexing() in each test
					return session;
				});
			return services;
		}
	}
}
