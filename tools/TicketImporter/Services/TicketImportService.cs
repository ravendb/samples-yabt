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
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class TicketImportService : IHostedService
	{
		private readonly IGitHubService _gitHubService;
		private readonly IBacklogItemCommandService _backlogItemService;
		private readonly AppSettings _settings;

		public TicketImportService(IGitHubService gitHubService, AppSettings settings, IBacklogItemCommandService backlogItemService)
		{
			_gitHubService = gitHubService;
			_settings = settings;
			_backlogItemService = backlogItemService;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			foreach (var repo in _settings.GitHub.Repos)
			{
				List<IssueResponse> l = new List<IssueResponse>();
				await foreach (var issues in _gitHubService.GetIssues(repo, 200, cancellationToken).WithCancellation(cancellationToken))
					foreach (var issue in issues)
					{
						BacklogItemAddUpdRequestBase dto;
						if (issue.Labels.Any(l => l.Name == "bug"))
							dto = new BugAddUpdRequest 
							{
								Title = issue.Title,
								StepsToReproduce = issue.Body,
								
							};
						else
							dto = new UserStoryAddUpdRequest 
							{
								Title = issue.Title,
								AcceptanceCriteria = issue.Body
							};

						var importResult = await _backlogItemService.Create(dto);
						if (!importResult.IsSuccess)
							throw new Exception($"Failed to import issue #{issue.Number}");
					}
			}

			throw new NotImplementedException();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		private T ConvertToBacklogItem<T>(IssueResponse issue) where T : BacklogItemAddUpdRequestBase, new()
		{
			return new T
			{
				Title = issue.Title
			};
		}
	}
}
