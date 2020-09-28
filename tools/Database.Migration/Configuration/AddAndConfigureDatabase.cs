using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Configuration;

namespace Raven.Yabt.Database.Migration.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register the document store as single instance
		/// </summary>
		public static void AddAndConfigureDatabase(this IServiceCollection services)
		{
			services.AddSingleton(x =>
				{
					var config = x.GetService<DatabaseSettings>();
					var store = SetupDocumentStore.GetDocumentStore(config.RavenDbUrls, config.Certificate, config.DbName);
					store.PreInitializeDocumentStore();
					return store.Initialize();
				});
		}
	}
}
