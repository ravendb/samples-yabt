using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Authorization;
using Raven.Yabt.WebApi.Configuration;
using Raven.Yabt.WebApi.Infrastructure;

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
			services.AddAndConfigureAppSettings(_configuration);

			services.AddControllers(o =>
						{
							// Register a filter to manage RavenDB session
							o.Filters.Add<DbSessionManagementFilter>();

							// Force all API methods to require authentication
							AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
							o.Filters.Add(new GlobalAuthorizeFilter(policy));
						})
					.AddJsonOptions(o =>
						{
							o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
							o.JsonSerializerOptions.IgnoreNullValues = true;
						});

			services.AddApplicationInsightsTelemetry();

			// Rigister all domain dependencies
			services.RegisterModules(assembly: Assembly.GetAssembly(typeof(ModuleRegistrationBase))!);

			// Register the database and DB session
			services.AddAndConfigureDatabase();

			// Register authentication
			services.AddAndConfigureAuthentication();
			// Register Swagger
			services.AddAndConfigureSwagger();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();

			app.ConfigureSwagger();

			app.UseEndpoints(endpoints =>endpoints.MapControllers());
		}
	}
}
