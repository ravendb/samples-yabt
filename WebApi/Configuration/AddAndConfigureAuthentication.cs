﻿using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.WebApi.Authorization;
using Raven.Yabt.WebApi.Authorization.ApiKeyAuth;

namespace Raven.Yabt.WebApi.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Add authentication services to the container
		/// </summary>
		public static void AddAndConfigureAuthentication(this IServiceCollection services)
		{
			services.AddScoped<ICurrentUserResolver, CurrentUserResolver>();

			services.AddAuthentication(sharedOptions =>
				{
					sharedOptions.DefaultScheme = PredefinedUserApiKeyAuthOptions.DefaultScheme;
				})
				// Add support for API key authentication
				.AddScheme<PredefinedUserApiKeyAuthOptions, PredefinedUserApiKeyAuthHandler>(PredefinedUserApiKeyAuthOptions.DefaultScheme, _=>{});
		}
	}
}