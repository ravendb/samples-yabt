using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common.Configuration;
using Raven.Yabt.Database.Infrastructure;

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
					return SetupDocumentStore.GetDocumentStore(config!);
				});
		}
	}
}
