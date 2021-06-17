using System;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common.Configuration;
using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.WebApi.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Add http clients to the container
		/// </summary>
		public static void AddAndConfigureHttpClients(this IServiceCollection services)
		{
			services.AddHttpClient<IRavenService, RavenService>((serviceProvider, client) =>
				{
					var dbSettings = serviceProvider.GetService<DatabaseSettings>();

					client.BaseAddress = new Uri(dbSettings!.RavenDbUrls[0]);
				});
		}
	}
}
