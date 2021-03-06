using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Configuration;

namespace Raven.Yabt.WebApi
{
	public class Startup
	{
		private readonly IConfiguration _configuration;
		private readonly IWebHostEnvironment _hostingEnvironment;

		public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
		{
			_configuration = configuration;
			_hostingEnvironment = hostingEnvironment;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Register Global Settings.
			var settings = services.AddAndConfigureAppSettings(_configuration);

			// Register controllers with filters, CORS settings, static files (for SPA)
			services.AddAndConfigureControllersAndStaticFiles(settings.CorsOrigins, settings.SpaRootPath);

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

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			app.UseHttpsRedirection();

			app.UseStaticFiles();
			if (!_hostingEnvironment.IsDevelopment())
				app.UseSpaStaticFiles();
			
			app.UseRouting();
			app.UseCors();

			app.UseAuthentication();

			app.ConfigureSwagger();

			app.UseEndpoints(endpoints =>endpoints.MapControllers());

			app.UseSpa(_ => {});
		}
	}
}
