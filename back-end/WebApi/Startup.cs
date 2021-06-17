using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Configuration;
using Raven.Yabt.WebApi.Configuration.Settings;
using Raven.Yabt.WebApi.Infrastructure;
using Raven.Yabt.WebApi.Infrastructure.StartupTasks;

namespace Raven.Yabt.WebApi
{
	public class Startup
	{	
		private readonly IConfiguration _configuration;
		private readonly IHostEnvironment _env;

		public Startup(IConfiguration configuration, IHostEnvironment env)
		{
			_configuration = configuration;
			_env = env;
		}

		/// <summary>
		///		This method gets called by the runtime. Use this method to add services to the container.
		/// </summary>
		public void ConfigureServices(IServiceCollection services)
		{
			// Register Global Settings.
			var settings = services.AddAndConfigureAppSettings(_configuration);

			// Register controllers with filters, CORS settings
			services.AddAndConfigureControllers(settings.CorsOrigins);

			// Register authentication
			services.AddAndConfigureAuthentication();

			// Register the database and DB session
			services.AddAndConfigureDatabase();
			
			// A start-up task to update DB indexes shouldn't be executed in PROD as it's a migration concern,
			// but registering it here makes dev live a bit easier by applying index updates on a start-up
			if (!_env.IsProduction())
				services.AddStartupTask<DbStartupTask>();

			// Register Swagger
			services.AddAndConfigureSwagger(settings.UserApiKey);

			// Register all domain dependencies
			services.RegisterModules(assembly: Assembly.GetAssembly(typeof(ModuleRegistrationBase))!);

			services.AddApplicationInsightsTelemetry();

			// Register all Http Clients
			services.AddAndConfigureHttpClients();
		}

		/// <summary>
		///		This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppSettings appSettings)
		{
			if (env.IsDevelopment())
				// CORS is used for development only 
				app.UseCors();
			else
			{	// Enforce use of HTTPS
				app.UseHsts();
				app.UseHttpsRedirection();
			}

			app.AddAppExceptionHandler(env);

			app.UseAuthentication();

			app.AddAppSwaggerUi();

			// Matches request to an endpoint
			app.UseRouting();
			// Executes the matched endpoint
			app.UseEndpoints(endpoints => 
				endpoints.MapControllers() // Maps attributes on the controllers, like, [Route], [HttpGet], etc.
			);

			app.AddAppSpaStaticFiles(appSettings.UserApiKey);
		}
	}
}
