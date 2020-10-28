using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;
using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Infrastructure;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class TicketImportService
	{
		private readonly IGitHubService _gitHubService;
		private readonly IBacklogItemCommandService _backlogItemService;
		private readonly IBacklogItemCommentCommandService _backlogCommentService;
		private readonly ISeededUsers _seededUser;
		private readonly IAsyncDocumentSession _dbSession;
		private readonly GitHubSettings _gitHubSettings;

		private readonly Regex _mentionRegex = new Regex(@"(?<=\B\@)([\w\._\-\/]+)", RegexOptions.Compiled);	// Get any word starting with '@'

		public TicketImportService(	AppSettings settings, 
									IGitHubService gitHubService, 
									IBacklogItemCommandService backlogItemService,
									IBacklogItemCommentCommandService backlogCommentService,
									ISeededUsers seededUser,
									IAsyncDocumentSession dbSession)
		{
			_gitHubService = gitHubService;
			_gitHubSettings = settings.GitHub;
			_dbSession = dbSession;
			_backlogItemService = backlogItemService;
			_backlogCommentService = backlogCommentService;
			_seededUser = seededUser;
		}

		public async Task Run(CancellationToken cancellationToken)
		{
			var userReferences = await _seededUser.GetGeneratedUsers();
			await _dbSession.SaveChangesAsync(cancellationToken);

			foreach (var repo in _gitHubSettings.Repos)
			{
				await foreach (var issues in _gitHubService.GetIssues(repo, _gitHubSettings.MaxImportedIssues, cancellationToken).WithCancellation(cancellationToken))
				{
					foreach (var issue in issues)
					{
						BacklogItemAddUpdRequestBase dto;
						if (issue.Labels.Any(l => l.Name == "bug"))
						{
							dto = ConvertToBacklogItem<BugAddUpdRequest>(issue, userReferences, 
									d => {
											var rnd = new Random();
											d.StepsToReproduce = issue.Body; 
											d.Severity = (BugSeverity)rnd.Next(Enum.GetNames(typeof(BugSeverity)).Length);
											d.Priority = (BugPriority)rnd.Next(Enum.GetNames(typeof(BugPriority)).Length);
										}
								);
						}
						else
							dto = ConvertToBacklogItem<UserStoryAddUpdRequest>(issue, userReferences, d => d.AcceptanceCriteria = issue.Body);

						var importResult = await _backlogItemService.Create(dto);
						if (!importResult.IsSuccess)
							throw new Exception($"Failed to import issue #{issue.Number}");

						if (issue.CommentsCount > 0)
							foreach (var commentDto in issue.Comments.Select(comment => ConvertToComment(comment, userReferences)))
							{
								await _backlogCommentService.Create(importResult.Value.Id!, commentDto);
							}
					}
					await _dbSession.SaveChangesAsync(cancellationToken);
				}
			}
		}

		private static T ConvertToBacklogItem<T>(IssueResponse issue, IList<UserReference> userReferences, Action<T> settingExtraFields) where T : BacklogItemAddUpdRequestBase, new()
		{
			var dto = new T { Title = issue.Title };
			if (issue.Labels?.Any() == true)
				dto.Tags = issue.Labels.Select(l => l.Name).ToArray();
			if (userReferences.Any())
				dto.AssigneeId = userReferences.OrderBy(x => Guid.NewGuid()).First().Id;

			settingExtraFields?.Invoke(dto);

			return dto;
		}

		private CommentAddUpdRequest ConvertToComment(CommentResponse comment, IList<UserReference> userReferences)
		{
			var matches = _mentionRegex.Matches(comment.Body);
			var references = matches.Distinct().Select(m => m.Value).ToArray();

			// Replace referred users to random users
			var body = comment.Body;
			foreach (var reference in references)
			{
				body = body.Replace(reference, userReferences.OrderBy(x => Guid.NewGuid()).First().MentionedName);
			}

			var dto = new CommentAddUpdRequest { Message = body };

			return dto;
		}
	}
}
