using System;
using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Configuration;

namespace Raven.Yabt.WebApi
{
	public class Startup
	{	
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
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

			// Register Swagger
			services.AddAndConfigureSwagger();

			// Register all domain dependencies
			services.RegisterModules(assembly: Assembly.GetAssembly(typeof(ModuleRegistrationBase))!);

			services.AddApplicationInsightsTelemetry();
		}

		/// <summary>
		///		This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		/// </summary>
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseHttpsRedirection();
			
			app.AddAppExceptionHandler(env);

			app.UseRouting();
			app.UseCors();

			app.UseAuthentication();

			app.AddAppSwaggerUi();

			app.UseEndpoints(endpoints => endpoints.MapControllers());

			app.AddAppSpaStaticFiles();
		}
	}
}
