using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Infrastructure;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class TicketImportService
	{
		private readonly IGitHubService _gitHubService;
		private readonly IBacklogItemCommandService _backlogItemService;
		private readonly ISeededUsers _seededUser;
		private readonly IAsyncDocumentSession _dbSession;
		private readonly string[] _gitHubRepos;

		public TicketImportService(	AppSettings settings, 
									IGitHubService gitHubService, 
									IBacklogItemCommandService backlogItemService,
									ISeededUsers seededUser,
									IAsyncDocumentSession dbSession)
		{
			_gitHubService = gitHubService;
			_gitHubRepos = settings.GitHub.Repos;
			_dbSession = dbSession;
			_backlogItemService = backlogItemService;
			_seededUser = seededUser;
		}

		public async Task Run(CancellationToken cancellationToken)
		{
			var userIds = await _seededUser.GetGeneratedUsers();
			await _dbSession.SaveChangesAsync();

			foreach (var repo in _gitHubRepos)
			{
				List<IssueResponse> l = new List<IssueResponse>();
				await foreach (var issues in _gitHubService.GetIssues(repo, 200, cancellationToken).WithCancellation(cancellationToken))
				{
					foreach (var issue in issues)
					{
						BacklogItemAddUpdRequestBase dto;
						if (issue.Labels.Any(l => l.Name == "bug"))
						{
							dto = ConvertToBacklogItem<BugAddUpdRequest>(issue, userIds, 
									d => {
											var rnd = new Random();
											d.StepsToReproduce = issue.Body; 
											d.Severity = (BugSeverity)rnd.Next(Enum.GetNames(typeof(BugSeverity)).Length);
											d.Priority = (BugPriority)rnd.Next(Enum.GetNames(typeof(BugPriority)).Length);
										}
								);
						}
						else
							dto = ConvertToBacklogItem<UserStoryAddUpdRequest>(issue, userIds, d => d.AcceptanceCriteria = issue.Body);

						var importResult = await _backlogItemService.Create(dto);
						if (!importResult.IsSuccess)
							throw new Exception($"Failed to import issue #{issue.Number}");
					}
					await _dbSession.SaveChangesAsync();
				}
			}
		}

		private T ConvertToBacklogItem<T>(IssueResponse issue, IEnumerable<string> userIds, Action<T> settingExtraFields) where T : BacklogItemAddUpdRequestBase, new()
		{
			var dto = new T { Title = issue.Title };
			if (issue.Labels?.Any() == true)
				dto.Tags = issue.Labels.Select(l => l.Name).ToArray();
			if (userIds.Any())
				dto.AssigneeId = userIds.OrderBy(x => Guid.NewGuid()).First();

			settingExtraFields?.Invoke(dto);

			return dto;
		}
	}
}
