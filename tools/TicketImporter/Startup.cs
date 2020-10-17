using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Services;

namespace Raven.Yabt.TicketImporter
{
	static class Startup
	{
		///  <summary>
		/// 	Register all the service for the appliction
		///  </summary>
		///  <remarks>
		///		See details on the approach at https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1#default-builder-settings
		///  </remarks>
		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return Host.CreateDefaultBuilder(args)
					.ConfigureAppConfiguration((builderContext, config) =>
					{
						// As I don't use 'DOTNET_ENVIRONMENT' reserved for console apps and rely on 'ASPNETCORE_ENVIRONMENT' instead, then
						//  a) need to set the environment manually
						builderContext.HostingEnvironment.EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
						//  b) add user secrets, as at the time 'CreateDefaultBuilder()' called it the environment was 'PROD'
						if (builderContext.HostingEnvironment.IsDevelopment())
							config.AddUserSecrets<Program>();
					})
					.ConfigureServices((context, services) =>
					{
						services.AddAndConfigureAppSettings(context.Configuration);
						services.AddAndConfigureHttpClients();
						services.AddHostedService<TicketDownloadService>();
					});
		}
	}
}
