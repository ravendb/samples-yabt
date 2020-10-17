using System.Net.Http;

using Microsoft.Extensions.Logging;

using Raven.Yabt.TicketImporter.Configuration;

namespace Raven.Yabt.TicketImporter.Infrastructure
{
	internal class GitHubService : IGitHubService
	{
		private readonly HttpClient _httpClient;
		private readonly AppSettings _settings;
		private readonly ILogger<GitHubService> _logger;

		public GitHubService(HttpClient httpClient, AppSettings settings, ILogger<GitHubService> logger)
		{
			_httpClient = httpClient;
			_settings = settings;
			_logger = logger;
		}
	}
}
