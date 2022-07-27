using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Raven.Yabt.WebApi.Configuration.Settings;

namespace Raven.Yabt.WebApi.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register Global Settings
	/// </summary>
	public static AppSettings AddAndConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions();

		services.Configure<AppSettings>(configuration, c => c.BindNonPublicProperties = true);
		services.AddSingleton(r => r.GetRequiredService<IOptions<AppSettings>>().Value);
		services.AddSingleton(r => r.GetRequiredService<AppSettings>().Database);
		services.AddSingleton(r => r.GetRequiredService<AppSettings>().DatabaseSession);
			
		// Makes IServiceProvider available in the container.
		var resolver = services.BuildServiceProvider();
		return resolver.GetService<AppSettings>()!;
	}
}