using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.WebApi.Authorization;
using Raven.Yabt.WebApi.Infrastructure;

namespace Raven.Yabt.WebApi.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register controllers with filters, CORS settings, etc.
	/// </summary>
	/// <param name="services"></param>
	/// <param name="corsOrigins"> CORS addresses as a ';'-separated string </param>
	public static void AddAndConfigureControllers(this IServiceCollection services, string corsOrigins)
	{
		services.AddControllers(o =>
		        {
			        // Register a filter to manage RavenDB session
			        o.Filters.Add<DbSessionManagementFilter>();

			        // Force all API methods to require authentication
			        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
			        o.Filters.Add(new GlobalAuthorizeFilter(policy));
		        })
		        .AddJsonOptions(o =>
		        {
			        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
			        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		        });
			
		services.AddCors(c =>
		{
			c.AddDefaultPolicy(options => options
			                              .WithOrigins(corsOrigins.Split(';'))
			                              .AllowAnyMethod()
			                              .AllowAnyHeader()
			                              .AllowCredentials());
		});
	}
}