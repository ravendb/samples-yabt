using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.TicketImporter.Infrastructure;

namespace Raven.Yabt.TicketImporter.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register an HTTP service to work with GitHub
		/// </summary>
		public static void AddAndConfigureHttpClients(IServiceCollection services, AppSettings settings)
		{
			services.AddHttpClient<IGitHubService, GitHubService>(client =>
			{
				client.BaseAddress = new Uri("https://api.github.com");
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				var authBytes = Encoding.ASCII.GetBytes($"{settings.GitHub.ClientId}:{settings.GitHub.ClientSecret}");
				client.DefaultRequestHeaders.Authorization =
					new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));
			});
		}
	}
}
