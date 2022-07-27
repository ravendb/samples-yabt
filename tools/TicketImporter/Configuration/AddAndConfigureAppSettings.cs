using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Raven.Yabt.TicketImporter.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register Global Settings
	/// </summary>
	public static IServiceCollection AddAndConfigureAppSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddOptions();

		services.Configure<AppSettings>(configuration, c => c.BindNonPublicProperties = true);
		services.AddSingleton(r => r.GetRequiredService<IOptions<AppSettings>>().Value);
		services.AddSingleton(r => r.GetRequiredService<AppSettings>().GitHub);
		services.AddSingleton(r => r.GetRequiredService<AppSettings>().GeneratedRecords);
		services.AddSingleton(r => r.GetRequiredService<AppSettings>().Database);

		return services;
	}
}