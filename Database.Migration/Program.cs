using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Migrations;
using Raven.Yabt.Database.Migration;
using Raven.Yabt.Database.Migration.Configuration;

namespace Database.Migration
{
	class Program
	{
		public static async Task<int> Main(string[] args)
		{
			using (var host = CreateHostBuilder(args).Build())
			{
				await host.StartAsync();
				var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

				// insert other console app code here

				lifetime.StopApplication();
				await host.WaitForShutdownAsync();
			}
			return 0;
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.UseConsoleLifetime()
				.ConfigureServices(ConfigureServices);

		private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
		{
			// Register appsettings.json
			services.AddAndConfigureAppSettings(context);
			// Register the Migration service
			services.AddHostedService<MigrationService>();
			// Register the document store
			services.AddAndConfigureDatabase();
			// Add the MigrationRunner into the dependency injection container.
			services.AddRavenDbMigrations();
		}
	}
}
