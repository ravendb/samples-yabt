using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Raven.Yabt.Database.Migration.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register Global Settings
		/// </summary>
		public static void AddAndConfigureAppSettings(this IServiceCollection services, HostBuilderContext context)
		{
			services.AddOptions();

			services.Configure<AppSettings>(context.Configuration, c => c.BindNonPublicProperties = true);
			services.AddSingleton(r => r.GetRequiredService<IOptions<AppSettings>>().Value.Database);
		}
	}
}
