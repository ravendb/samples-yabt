using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Configuration;
using Raven.Yabt.WebApi.Configuration.Settings;

namespace Raven.Yabt.WebApi;

public class Startup
{
	private readonly IConfiguration _configuration;

	public Startup(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	/// <summary>
	///		This method gets called by the runtime to add services to the container.
	/// </summary>
	public void ConfigureServices(IServiceCollection services)
	{
		// Register Global Settings.
		var settings = services.AddAndConfigureAppSettings(_configuration);

		// Register controllers with filters, CORS settings
		services.AddAndConfigureControllers(settings.CorsOrigins);

		// Register authentication
		services.AddAndConfigureAuthentication();

		// Register the domain services along with the database and DB session
		services.AddAndConfigureDomainServices(true);

		// Register Swagger
		services.AddAndConfigureSwagger(settings.UserApiKey);

		services.AddApplicationInsightsTelemetry();
	}

	/// <summary>
	///		This method gets called by the runtime to configure the HTTP request pipeline.
	/// </summary>
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AppSettings appSettings)
	{
		if (env.IsDevelopment())
			// CORS is used for development only 
			app.UseCors();
		else
		{
			// Enforce use of HTTPS
			app.UseHsts();
			app.UseHttpsRedirection();
		}

		app.AddAppExceptionHandler(env);

		app.UseAuthentication();

		app.AddAppSwaggerUi();

		// Matches request to an endpoint
		app.UseRouting();
		// Executes the matched endpoint
		app.UseEndpoints(
				endpoints => endpoints.MapControllers() // Maps attributes on the controllers, like, [Route], [HttpGet], etc.
			);

		app.AddAppSpaStaticFiles(appSettings.UserApiKey);
	}
}