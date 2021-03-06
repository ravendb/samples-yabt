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
		public void Configure(IApplicationBuilder app)
		{
			app.UseHttpsRedirection();

			// Serve files inside of web root (wwwroot folder)
			app.UseStaticFiles();
			
			app.UseRouting();
			app.UseCors();

			app.UseAuthentication();

			app.ConfigureSwagger();

			app.UseEndpoints(endpoints =>endpoints.MapControllers());

			// Does 2 things (see details at https://stackoverflow.com/a/56977859/968003):
			//	- Rewrites all requests to the default page;
			//	- Tries to configure static files serving (falls back to UseSpaStaticFiles() and serving them from 'wwwroot')
			app.UseSpa(_ => {});
		}
	}
}
