using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;
using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Infrastructure;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class TicketImportService
	{
		private readonly IGitHubService _gitHubService;
		private readonly ICustomFieldListQueryService _customFieldQueryService;
		private readonly ICustomFieldCommandService _customFieldCmdService;
		private readonly IBacklogItemCommandService _backlogItemService;
		private readonly IBacklogItemCommentCommandService _backlogCommentService;
		private readonly ISeededUsers _seededUser;
		private readonly IAsyncDocumentSession _dbSession;
		private readonly AppSettings _settings;

		private readonly Regex _mentionRegex = new (@"(?<=\B\@)([\w\._\-\/]+)", RegexOptions.Compiled);	// Get any word starting with '@'
		private readonly string urlCustomFieldName = "Original URL";

		private readonly List<string> _createdTicketIds = new ();
		
		public TicketImportService(	AppSettings settings, 
									IGitHubService gitHubService, 
									ICustomFieldCommandService customFieldCmdService,
									ICustomFieldListQueryService customFieldQueryService,
									IBacklogItemCommandService backlogItemService,
									IBacklogItemCommentCommandService backlogCommentService,
									ISeededUsers seededUser,
									IAsyncDocumentSession dbSession
								)
		{
			_gitHubService = gitHubService;
			_settings = settings;
			_dbSession = dbSession;
			_customFieldQueryService = customFieldQueryService;
			_customFieldCmdService = customFieldCmdService;
			_backlogItemService = backlogItemService;
			_backlogCommentService = backlogCommentService;
			_seededUser = seededUser;
		}

		public async Task Run(CancellationToken cancellationToken)
		{
			// Generate or fetch the users
			var userReferences = await _seededUser.GetGeneratedOrFetchedUsers();
			await SaveChanges(cancellationToken);
			
			// Generate or fetch the Custom Field for preserving the reference to the original ticket
			var customFieldId = await GenerateOrFetchUrlCustomField();
			await SaveChanges(cancellationToken);

			var gitHubUrls = await GetGitHubUrlsForExistingTickets(customFieldId);

			// Iterate through the GitHub repos and import tickets with comments  
			foreach (var repo in _settings.GitHub.Repos)
			{
				await foreach (var issues in _gitHubService.GetIssues(repo, _settings.GitHub.MaxImportedIssues, cancellationToken).WithCancellation(cancellationToken))
				{
					var ticketIdPerIteration = new List<string>();
					foreach (var issue in issues)
					{
						var gitHubUrl = $"https://github.com/{repo}/issues/{issue.Number}";
						if (gitHubUrls.Contains(gitHubUrl))
							continue;
						
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
								BacklogItemType.Feature => ConvertToBacklogItem<TaskAddUpdRequest>(issue, userReferences, (customFieldId, gitHubUrl), 
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
					}
					await SaveChanges(cancellationToken);
					_createdTicketIds.AddRange(ticketIdPerIteration);
				}
			}
		}

		private async Task<string[]> GetGitHubUrlsForExistingTickets(string customFieldId)
		{
			var cf = await (
					from t in _dbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
					where t.CustomFields![customFieldId] != null
					select t.CustomFields!
				).ToArrayAsync();
			return cf.Select(c => c[customFieldId].ToString()).ToArray();
		}

		private T ConvertToBacklogItem<T>(IssueResponse issue, IList<UserReference> userReferences, (string, string) customField, Action<T>? settingExtraFields) where T : BacklogItemAddUpdRequestBase, new()
		{
			var rnd = new Random();
			var dto = new T { Title = issue.Title };
			if (issue.Labels?.Any() == true)
				dto.Tags = issue.Labels.Select(l => l.Name).ToArray();
			
			if (userReferences.Any() && rnd.NextDouble() < _settings.GeneratedRecords.PartOfAssignedTickets)
				dto.AssigneeId = userReferences.OrderBy(_ => Guid.NewGuid()).First().Id;

			var (customFieldId, customFieldValue) = customField;
			dto.ChangedCustomFields = new List<BacklogCustomFieldAction> { new() { CustomFieldId = customFieldId, Value = customFieldValue, ActionType = ListActionType.Add } };
			
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
			await _dbSession.SaveChangesAsync(cancellationToken);
			_dbSession.Advanced.Clear(); // Clear all cached entities
		}

		private static BacklogItemType GetBacklogItemType(IssueResponse issue)
		{
			return 
				issue.Labels.Any(l => l.Name == "bug") ? BacklogItemType.Bug :
				issue.Labels.Any(l => l.Name == "task") ? BacklogItemType.Task : 
				issue.Labels.Any(l => l.Name == "feature") ? BacklogItemType.Feature : BacklogItemType.UserStory; 
		} 
		
		private async Task<string> GenerateOrFetchUrlCustomField()
		{
			// Return the ID of the existing field (if exists)
			var existingFields = await _customFieldQueryService.GetArray(new CustomFieldListGetRequest { PageSize = int.MaxValue });
			var field = existingFields.FirstOrDefault(f => f.FieldType == CustomFieldType.Url && f.Name == urlCustomFieldName);
			if (field is not null)
				return field.Id!;
			
			// Create a new field
			var (fieldRef, _) = await _customFieldCmdService.Create(new CustomFieldAddRequest { FieldType = CustomFieldType.Url, Name = urlCustomFieldName, IsMandatory = false });
			return fieldRef.Id;
		}
	}
}
