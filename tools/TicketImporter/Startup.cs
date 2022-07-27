using Microsoft.Extensions.Hosting;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.TicketImporter.Configuration;

namespace Raven.Yabt.TicketImporter;

static class Startup
{
	///  <summary>
	/// 	Register all the service for the application
	///  </summary>
	///  <remarks>
	///		See details on the approach at https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1#default-builder-settings
	///  </remarks>
	public static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args)	//  Since .NET Core 3, the generic host uses the DOTNET_ prefix (not the old ASPNETCORE_). It also adds user secrets for dev environment (https://stackoverflow.com/a/60207531/968003)
		           .ConfigureServices((context, services) =>
		           {
			           services.AddAndConfigureAppSettings(context.Configuration)
			                   .AddAndConfigureHttpClients()
			                   .AddAndConfigureDatabaseSessionForImport()
			                   .AddAndConfigureAuthentication()
			                   .AddAndConfigureJobServices()
			                   .AddAndConfigureDomainServices(false);
		           });
		// The app starts executing classes implementing 'IHostedService' (e.g. 'HostedServiceWrapper')
	}
}