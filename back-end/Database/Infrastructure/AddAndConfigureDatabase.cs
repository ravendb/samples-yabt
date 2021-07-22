using System;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Database.Common.Configuration;

namespace Raven.Yabt.Database.Infrastructure
{
	public static class ServiceCollectionExtensions
	{
		public static void AddAndConfigureDatabase(this IServiceCollection services, Func<IServiceProvider, Func<string>> currentTenantResolverFunc)
		{
			// Register the document store as single instance, initializing it on first use
			services.AddSingleton<IDocumentStore>(x =>
				{
					var config = x.GetRequiredService<DatabaseSettings>();
					return SetupDocumentStore.GetDocumentStore(config);
				});
			
			// Register a tenanted session
			services.AddScoped<IAsyncTenantedDocumentSession>(x =>
				{
					var docStore				= x.GetRequiredService<IDocumentStore>();
					var config					= x.GetService<DatabaseSessionSettings>();
					var getCurrentTenantIdFunc	= currentTenantResolverFunc(x);

					TimeSpan? waitingForIndexesAfterSaveTimeSpan = config?.WaitForIndexesAfterSaveChanges > 0 ? TimeSpan.FromSeconds(config.WaitForIndexesAfterSaveChanges.Value) : null;
						
					return new AsyncTenantedDocumentSession(docStore, getCurrentTenantIdFunc, waitingForIndexesAfterSaveTimeSpan);
				});
		}
	}
}
