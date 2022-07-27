using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Authorization;
using Raven.Yabt.WebApi.Authorization.ApiKeyAuth;

namespace Raven.Yabt.WebApi.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Add authentication services to the container
	/// </summary>
	public static void AddAndConfigureAuthentication(this IServiceCollection services)
	{
		services.AddScoped<ICurrentUserResolver, CurrentUserResolver>();
		services.AddScoped<ICurrentTenantResolver, CurrentTenantResolver>();

		services.AddAuthentication(options =>
		        {
			        options.DefaultScheme = PredefinedUserApiKeyAuthOptions.DefaultScheme;
		        })
		        // Add support for API key authentication
		        .AddScheme<PredefinedUserApiKeyAuthOptions, PredefinedUserApiKeyAuthHandler>(PredefinedUserApiKeyAuthOptions.DefaultScheme, _=>{});
	}
}