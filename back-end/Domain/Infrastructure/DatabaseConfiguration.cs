using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Yabt.Database;
using Raven.Yabt.Database.Common.Configuration;

namespace Raven.Yabt.Domain.Infrastructure
{
	public static class DatabaseConfigurationExtension
	{
		/// <summary>
		///     Register RavenDB connection (aka Document Store) and a DB session (aka unit of work)
		/// </summary>
		public static void AddAndConfigureDatabase(this IServiceCollection services)
		{
			// Register the document store as single instance, initializing it on first use
			services.AddSingleton(x =>
				{
					var config = x.GetRequiredService<DatabaseSettings>();
					//var tenantResolver = x.GetService<ICurrentTenantResolver>();
					return SetupDocumentStore.GetDocumentStore(config);//, tenantResolver != null ? tenantResolver.GetCurrentTenantId : null);
				});
			
			// Register a session
			services.AddScoped(x =>
				{
					var config = x.GetService<DatabaseSessionSettings>();
					var session = x.GetRequiredService<IDocumentStore>().OpenAsyncSession();

					if (config?.WaitForIndexesAfterSaveChanges > 0) 
						// Wait on each change to avoid adding WaitForIndexing() in each test
						session.Advanced.WaitForIndexesAfterSaveChanges(
							TimeSpan.FromSeconds(config.WaitForIndexesAfterSaveChanges.Value), 
							false);

					return session;
				});
		}

		/// <summary>
		///		Creates/updates the DB indexes
		/// </summary>
		public static Task CreateUpdateIndexesAsync(this IDocumentStore store) =>
			IndexCreation.CreateIndexesAsync(typeof(SetupDocumentStore).Assembly, store, null, store.Database);
	}
}
