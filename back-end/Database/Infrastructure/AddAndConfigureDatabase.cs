using System;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Database.Common.Configuration;

namespace Raven.Yabt.Database.Infrastructure;

public static class ServiceCollectionExtensions
{
	/// <summary>
	///		Register the document store as single instance, initializing it on first use
	/// </summary>
	/// <param name="services"> The collection of services </param>
	/// <param name="customInit"> A way top optionally initialise additional properties </param>
	public static IServiceCollection AddAndConfigureDatabase(this IServiceCollection services, Action<IDocumentStore>? customInit = null)
	{
		return services.AddSingleton<IDocumentStore>(x =>
		{
			var config = x.GetRequiredService<DatabaseSettings>();
			return SetupDocumentStore.GetDocumentStore(config, customInit);
		});
	}
		
	/// <summary>
	///		Register a tenanted session
	/// </summary>
	/// <param name="services"> The collection of services </param>
	/// <param name="currentTenantResolverFunc"> A way to resolve the function for getting the current tenant </param>
	public static IServiceCollection AddAndConfigureDatabaseTenantedSession(this IServiceCollection services, Func<IServiceProvider, Func<string>> currentTenantResolverFunc)
	{
		return services.AddScoped<IAsyncTenantedDocumentSession>(x =>
		{
			var docStore				= x.GetRequiredService<IDocumentStore>();
			var config					= x.GetService<DatabaseSessionSettings>();
			var getCurrentTenantIdFunc	= currentTenantResolverFunc(x);

			TimeSpan? waitingForIndexesAfterSaveTimeSpan = config?.WaitForIndexesAfterSaveChanges > 0 ? TimeSpan.FromSeconds(config.WaitForIndexesAfterSaveChanges.Value) : null;
							
			return new AsyncTenantedDocumentSession(docStore, getCurrentTenantIdFunc, waitingForIndexesAfterSaveTimeSpan);
		});
	}
}