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
using Raven.Yabt.WebApi.Configuration.Settings;

namespace Raven.Yabt.WebApi
{
	public class Startup
	{
		private const string CookieNameApiBaseUrl = "apiBaseUrl";
		private const string CookieNameApiUserKeys = "apiUserKeys";
		
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
		/// <remarks>
		///		Links for 'UseStaticFiles' vs 'UseSpa' - https://stackoverflow.com/a/56977859/968003
		/// </remarks>
		public void Configure(IApplicationBuilder app, AppSettings settings)
		{
			app.UseHttpsRedirection();

			// Serve files inside of web root (wwwroot folder) other than 'index.html'
			// Without this method Kestrel would return 'index.html' on all the requests for static content 
			app.UseStaticFiles(
				// Set cache expiration for all static files except 'index.html'
				new StaticFileOptions
				{
					OnPrepareResponse = ctx =>
					{
						var headers = ctx.Context.Response.GetTypedHeaders();
						headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromDays(12*30) };
					}
				});
			
			app.UseRouting();
			app.UseCors();

			app.UseAuthentication();

			app.ConfigureSwagger();

			app.UseEndpoints(endpoints => endpoints.MapControllers());

			// Does 3 things:
			//	- Rewrites all requests to the default page;
			//	- Serves 'index.html'
			//	- Tries to configure static files serving (falls back to UseSpaStaticFiles() and serving them from 'wwwroot')
			app.UseSpa(
				// Disable cache for 'index.html' (https://stackoverflow.com/q/49547/968003)
				// The correct minimum set includes:
				//     Cache-Control: no-cache, no-store, must-revalidate, max-age=0
				c => c.Options.DefaultPageStaticFileOptions = new StaticFileOptions
				{
					OnPrepareResponse = ctx =>
					{
						var response = ctx.Context.Response; 
						response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue	// Supersedes 'Pragma' header (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Pragma)
						{
							NoCache = true,
							NoStore = true,
							MustRevalidate = true,
							MaxAge = TimeSpan.Zero	// Overtakes 'Expires: 0' (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Expires)
						};
						// The front-end is immutable, so it doesn't change on deployment.
						// Two options to pass the API URL into the SPA:
						//	a) inject into 'index.html' (would need add an MVC controller to serve altered 'index.html')
						//	b) pass into cookie (it's simple, see below) 
						response.Cookies.Append(CookieNameApiBaseUrl, "/");
					}
				});
		}
	}
}
