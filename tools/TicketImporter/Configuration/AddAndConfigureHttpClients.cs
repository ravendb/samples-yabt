using System;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.TicketImporter.Infrastructure;

namespace Raven.Yabt.TicketImporter.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register an HTTP service to work with GitHub
	/// </summary>
	public static IServiceCollection AddAndConfigureHttpClients(this IServiceCollection services)
	{
		services.AddHttpClient<IGitHubService, GitHubService>((serviceProvider, client) =>
		{
			var settings = serviceProvider.GetService<AppSettings>();

			client.BaseAddress = new Uri("https://api.github.com");
			client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
			client.DefaultRequestHeaders.Add("User-Agent", "TicketImporter");

			var authBytes = Encoding.ASCII.GetBytes($"{settings!.GitHub.ClientId}:{settings.GitHub.ClientSecret}");
			client.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
		});

		return services;
	}
}