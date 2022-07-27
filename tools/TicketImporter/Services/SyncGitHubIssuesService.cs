using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Infrastructure;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Services;

internal interface ISyncGitHubIssuesService
{
	Task CreateTicketsForGitHubIssues(string gitHubRepoName, IList<UserReference> userReferences, string customFieldId, string[] gitHubUrls, CancellationToken cancellationToken=default);
}

internal class SyncGitHubIssuesService : ISyncGitHubIssuesService
{
	private readonly IGitHubService _gitHubService;
	private readonly IBacklogItemCommandService _backlogItemService;
	private readonly IBacklogItemCommentCommandService _backlogCommentService;
	private readonly IAsyncTenantedDocumentSession _dbSession;
	private readonly AppSettings _settings;

	private readonly Regex _mentionRegex = new (@"(?<=\B\@)([\w\._\-\/]+)", RegexOptions.Compiled);	// Get any word starting with '@'

	private readonly List<string> _createdTicketIds = new ();
		
	public SyncGitHubIssuesService(AppSettings settings, 
	                               IGitHubService gitHubService,
	                               IBacklogItemCommandService backlogItemService,
	                               IBacklogItemCommentCommandService backlogCommentService,
	                               IAsyncTenantedDocumentSession dbSession)
	{
		_gitHubService = gitHubService;
		_settings = settings;
		_backlogItemService = backlogItemService;
		_backlogCommentService = backlogCommentService;
		_dbSession = dbSession;
	}

	public async Task CreateTicketsForGitHubIssues(string gitHubRepoName, IList<UserReference> userReferences, string customFieldId, string[] gitHubUrls, CancellationToken cancellationToken=default)
	{
		var repoUrl = $"https://github.com/{gitHubRepoName}/issues/";
		var validateIssue = new Func<IssueResponse, bool>(issue => !gitHubUrls.Contains($"{repoUrl}{issue.Number}"));
			
		await foreach (var issues in _gitHubService.GetIssuesAsync(gitHubRepoName, validateIssue, cancellationToken).WithCancellation(cancellationToken))
		{
			var ticketIdPerIteration = new List<string>();
			foreach (var issue in issues)
			{
				if (cancellationToken.IsCancellationRequested) return;
						
				var gitHubUrl = $"{repoUrl}{issue.Number}";
					
				BacklogItemAddUpdRequestBase dto = 
					GetBacklogItemType(issue) switch
					{
						BacklogItemType.Bug => ConvertToBacklogItem<BugAddUpdRequest>(issue, userReferences, (customFieldId, gitHubUrl),
							d =>
							{
								var rnd = new Random();
								d.StepsToReproduce = issue.Body;
								d.Severity = (BugSeverity) rnd.Next(Enum.GetNames(typeof(BugSeverity)).Length);
								d.Priority = (BugPriority) rnd.Next(Enum.GetNames(typeof(BugPriority)).Length);
							}),
						BacklogItemType.UserStory => ConvertToBacklogItem<UserStoryAddUpdRequest>(issue, userReferences, (customFieldId, gitHubUrl),
								d => d.AcceptanceCriteria = issue.Body
							),
						BacklogItemType.Task => ConvertToBacklogItem<TaskAddUpdRequest>(issue, userReferences, (customFieldId, gitHubUrl),
								d => d.Description = issue.Body
							),
						BacklogItemType.Feature => ConvertToBacklogItem<FeatureAddUpdRequest>(issue, userReferences, (customFieldId, gitHubUrl), 
								d => d.Description = issue.Body
							),
						_ => throw new NotImplementedException("Type not supported")
					};

				var (createdTicketRef, status) = await _backlogItemService.Create(dto);
				if (!status.IsSuccess)
					throw new Exception($"Failed to import issue #{issue.Number}");
				ticketIdPerIteration.Add(createdTicketRef.Id!);

				if (issue.CommentsCount > 0)
					foreach (var commentDto in issue.Comments.Select(comment => ConvertToComment(comment, userReferences)))
					{
						await _backlogCommentService.Create(createdTicketRef.Id!, commentDto);
					}

				if (issue.State == "closed")
				{
					await SaveChanges(cancellationToken);
					await _backlogItemService.SetState(createdTicketRef.Id!, BacklogItemState.Closed);
				}
			}
			await SaveChanges(cancellationToken);
			_createdTicketIds.AddRange(ticketIdPerIteration);
		}
	}

	private T ConvertToBacklogItem<T>(IssueResponse issue, IList<UserReference> userReferences, (string, string) customField, Action<T>? settingExtraFields) where T : BacklogItemAddUpdRequestBase, new()
	{
		var rnd = new Random();
		var dto = new T { Title = issue.Title };
		if (issue.Labels?.Any() == true)
		{
			// Sanitise labels
			var labels = from l in issue.Labels.Select(l => Regex.Replace(l.Name, "(feature|bug|task|user\\sstory|area)-{0,1}",""))
				where l.Length is > 0 and < 12 
					&& !l.Any(c => c is ':' or '*')
				select l;
			dto.Tags = labels.Distinct().ToArray();
		}
			
		if (userReferences.Any() && rnd.NextDouble() < _settings.GeneratedRecords.PartOfAssignedTickets)
			dto.AssigneeId = userReferences.OrderBy(_ => Guid.NewGuid()).First().Id;

		var (customFieldId, customFieldValue) = customField;
		dto.ChangedCustomFields = new List<BacklogCustomFieldAction> { new() { CustomFieldId = customFieldId, ObjValue = customFieldValue, ActionType = ListActionType.Add } };
			
		if (_createdTicketIds.Count > 1 && rnd.NextDouble() < _settings.GeneratedRecords.PartOfTicketsWithRelatedItems)
		{
			dto.ChangedRelatedItems = new List<BacklogRelationshipAction>
			{
				new()
				{
					BacklogItemId = _createdTicketIds[rnd.Next(_createdTicketIds.Count-1)], 
					ActionType = ListActionType.Add, 
					RelationType = (BacklogRelationshipType) rnd.Next(Enum.GetNames(typeof(BacklogRelationshipType)).Length)
				}
			};
		}
			
		settingExtraFields?.Invoke(dto);

		return dto;
	}

	private string ConvertToComment(CommentResponse comment, IList<UserReference> userReferences)
	{
		var matches = _mentionRegex.Matches(comment.Body);
		var references = matches.Distinct().Select(m => m.Value).ToArray();

		// Replace referred users to random users
		var body = comment.Body;
		foreach (var reference in references)
		{
			body = body.Replace(reference, userReferences.OrderBy(_ => Guid.NewGuid()).First().MentionedName);
		}
		return body;
	}

	private async Task SaveChanges(CancellationToken cancellationToken)
	{
		await _dbSession.SaveChangesAsync(true, cancellationToken);
	}

	private static BacklogItemType GetBacklogItemType(IssueResponse issue)
	{
		return 
			issue.Labels.Any(l => l.Name == "bug") ? BacklogItemType.Bug :
			issue.Labels.Any(l => l.Name == "task") ? BacklogItemType.Task : 
			issue.Labels.Any(l => l.Name.StartsWith("feature")) ? BacklogItemType.Feature : BacklogItemType.UserStory; 
	}
}