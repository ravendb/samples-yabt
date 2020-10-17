using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Infrastructure;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class TicketDownloadService : IHostedService
	{
		private readonly IGitHubService _gitHubService;
		private readonly AppSettings _settings;

		public TicketDownloadService(IGitHubService gitHubService, AppSettings settings)
		{
			_gitHubService = gitHubService;
			_settings = settings;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			foreach (var repo in _settings.GitHub.Repos)
			{
				List<IssueResponse> l = new List<IssueResponse>();
				await foreach (var issues in _gitHubService.GetIssues(repo, 200, cancellationToken).WithCancellation(cancellationToken))
				{
					l.AddRange(issues);
				}
			}

			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
