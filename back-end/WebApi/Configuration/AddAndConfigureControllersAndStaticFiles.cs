using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.WebApi.Authorization;
using Raven.Yabt.WebApi.Infrastructure;

namespace Raven.Yabt.WebApi.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register controllers with filters, CORS settings, static files (for SPA), etc.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="corsOrigins"> CORS addresses as a ';'-separated string </param>
		/// <param name="spaRootPath"> Path to the compiled SPA app </param>
		public static void AddAndConfigureControllersAndStaticFiles(this IServiceCollection services, string corsOrigins, string spaRootPath)
		{
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
			
			services.AddCors(c =>
					{
						c.AddDefaultPolicy(options => options
						                                              .WithOrigins(corsOrigins.Split(';'))
						                                              .AllowAnyMethod()
						                                              .AllowAnyHeader()
						                                              .AllowCredentials());
					});
			
			// In production, the Angular files will be served from this directory
			services.AddSpaStaticFiles(configuration => { configuration.RootPath = spaRootPath; });
		}
	}
}
