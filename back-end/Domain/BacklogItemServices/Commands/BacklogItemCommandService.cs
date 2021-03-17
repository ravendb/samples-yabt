using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.Query;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	public class BacklogItemCommandService : BaseService<BacklogItem>, IBacklogItemCommandService
	{
		private readonly IUserReferenceResolver _userResolver;
		private readonly ICustomFieldListQueryService _customFieldQueryService;

		public BacklogItemCommandService(IAsyncDocumentSession dbSession, IUserReferenceResolver userResolver, ICustomFieldListQueryService customFieldQueryService) : base(dbSession)
		{
			_userResolver = userResolver;
			_customFieldQueryService = customFieldQueryService;
		}

		public async Task<IDomainResult<BacklogItemReference>> Create<T>(T dto) where T : BacklogItemAddUpdRequestBase
		{
			BacklogItem? ticket = dto switch
			{
				BugAddUpdRequest bug		 => await ConvertDtoToEntity<BacklogItemBug,		BugAddUpdRequest>(bug),
				UserStoryAddUpdRequest story => await ConvertDtoToEntity<BacklogItemUserStory,	UserStoryAddUpdRequest>(story),
				TaskAddUpdRequest task		 => await ConvertDtoToEntity<BacklogItemTask,		TaskAddUpdRequest>(task),
				_ => null,
			};
			if (ticket == null)
				return DomainResult.Failed<BacklogItemReference>("Incorrect Backlog structure");

			await DbSession.StoreAsync(ticket);
			var ticketRef = ticket.ToReference().RemoveEntityPrefixFromId();

			UpdateRelatedItems(dto, ticketRef);
			
			return DomainResult.Success(ticketRef);
		}

		public async Task<IDomainResult<BacklogItemReference>> Update<T>(string id, T dto) where T : BacklogItemAddUpdRequestBase
		{
			var entity = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
			if (entity == null)
				return DomainResult.NotFound<BacklogItemReference>();

			entity = dto switch
			{
				BugAddUpdRequest bug			=> await ConvertDtoToEntity (bug,	entity as BacklogItemBug),
				UserStoryAddUpdRequest story	=> await ConvertDtoToEntity (story,	entity as BacklogItemUserStory),
				TaskAddUpdRequest task			=> await ConvertDtoToEntity (task,  entity as BacklogItemTask),
				_ => null
			};
			if (entity == null)
				return DomainResult.Failed<BacklogItemReference>("Incorrect Backlog structure");

			var ticketRef = entity.ToReference().RemoveEntityPrefixFromId();

			UpdateRelatedItems(dto, ticketRef);
			
			return DomainResult.Success(ticketRef);
		}

		public async Task<IDomainResult<BacklogItemReference>> Delete(string shortId)
		{
			var ticket = await DbSession.LoadAsync<BacklogItem>(GetFullId(shortId));
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemReference>();

			DbSession.Delete(ticket);

			// Remove references to the ticket from 'Related Items' of other tickets
			if (ticket.RelatedItems.Any())
				foreach (var item in ticket.RelatedItems)
				{
					DbSession.Advanced.Patch<BacklogItem, BacklogItemRelatedItem>(GetFullId(item.RelatedTo.Id!),
						x => x.RelatedItems,
						items => items.RemoveAll(i => i.RelatedTo.Id == shortId));
				}

			return DomainResult.Success(
									ticket.ToReference().RemoveEntityPrefixFromId()
								);
		}

		public async Task<IDomainResult<BacklogItemReference>> AssignToUser(string backlogItemId, string? userShortenId)
		{
			var backlogItem = await DbSession.LoadAsync<BacklogItem>(GetFullId(backlogItemId));
			if (backlogItem == null)
				return DomainResult.NotFound<BacklogItemReference>("The Backlog Item not found");

			if (userShortenId == null)
				backlogItem.Assignee = null;
			else
			{
				var userRef = await _userResolver.GetReferenceById(userShortenId);
				if (userRef == null)
					return DomainResult.NotFound<BacklogItemReference>("The user not found");

				backlogItem.Assignee = userRef.RemoveEntityPrefixFromId();
			}

			backlogItem.AddHistoryRecord(await _userResolver.GetCurrentUserReference(), "Assigned a user");

			return DomainResult.Success(
									backlogItem.ToReference().RemoveEntityPrefixFromId()
								);
		}

		private async Task<TModel> ConvertDtoToEntity<TModel, TDto>(TDto dto, TModel? entity = null)
			where TModel : BacklogItem, new()
			where TDto : BacklogItemAddUpdRequestBase
		{
			entity ??= new TModel();

			entity.Title = dto.Title;
			entity.State = dto.State;
			entity.EstimatedSize = dto.EstimatedSize;
			entity.Tags = dto.Tags;
			entity.Assignee = dto.AssigneeId != null ? await _userResolver.GetReferenceById(dto.AssigneeId) : null;
	
			entity.AddHistoryRecord(
				await _userResolver.GetCurrentUserReference(), 
				entity.ModifiedBy.Any() ? "Modified" : "Created"	// TODO: Provide more informative description in case of modifications
			);

			if (dto.CustomFields != null)
			{
				var verifiedCustomFieldIds = await _customFieldQueryService.VerifyExistingItems(
						dto.CustomFields.Where(pair => pair.Value != null).Select(pair => pair.Key)
					);
				entity.CustomFields = verifiedCustomFieldIds.ToDictionary(x => x, x => dto.CustomFields[x]!);
			}
			else
				entity.CustomFields = null;

			await ResolveChangedRelatedItems(entity.RelatedItems, dto.ChangedRelatedItems);

			if (dto is BugAddUpdRequest bugDto && entity is BacklogItemBug bugEntity)
			{
				bugEntity.Severity = bugDto.Severity;
				bugEntity.Priority = bugDto.Priority;
				bugEntity.StepsToReproduce = bugDto.StepsToReproduce;
				bugEntity.AcceptanceCriteria = bugDto.AcceptanceCriteria;
			}
			else if (dto is UserStoryAddUpdRequest storyDto && entity is BacklogItemUserStory storyEntity)
			{
				storyEntity.AcceptanceCriteria = storyDto.AcceptanceCriteria;
			}

			return entity;
		}

		private void UpdateRelatedItems<T>(T dto, BacklogItemReference ticketRef) where T : BacklogItemAddUpdRequestBase
		{
			if (dto.ChangedRelatedItems?.Any() != true) return;
			
			foreach (var link in dto.ChangedRelatedItems)
			{
				if (link.ActionType == BacklogRelationshipActionType.Add)
					DbSession.Advanced.Patch<BacklogItem, BacklogItemRelatedItem>(
						GetFullId(link.BacklogItemId),
						x => x.RelatedItems,
						items => items.Add(
							new BacklogItemRelatedItem
							{
								RelatedTo = ticketRef,
								LinkType = link.RelationType.GetMirroredType()
							}));
				else
				{
					var relatedId = ticketRef.Id!;
					var relationType = link.RelationType.GetMirroredType();
					DbSession.Advanced.Patch<BacklogItem, BacklogItemRelatedItem>(
						GetFullId(link.BacklogItemId),
						x => x.RelatedItems,
						items => items.RemoveAll(
							i => i.RelatedTo.Id! == relatedId && i.LinkType == relationType));
				}
			}
		}

		private async Task ResolveChangedRelatedItems(List<BacklogItemRelatedItem> existingRelatedItems, IList<BacklogRelationshipAction>? actions)
		{
			if (actions == null)
				return;

			// Remove 'old' links
			foreach (var (id, linkType) in from a in actions
												where a.ActionType == BacklogRelationshipActionType.Remove
												select (a.BacklogItemId, a.RelationType))
			{
				existingRelatedItems.RemoveAll(existing => existing.RelatedTo.Id == id && existing.LinkType == linkType);
			}
			
			// Add new links
			(string fullId, BacklogRelationshipType linkType)[] array = 
				(from a in actions
				 where a.ActionType == BacklogRelationshipActionType.Add
				 select (GetFullId(a.BacklogItemId), a.RelationType)
				 ).ToArray();
			if (array.Any())
			{
				// Resolve new references
				var fullIds = array.Select(a => a.fullId).Distinct();
				var references = await (from b in DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
					where b.Id.In(fullIds)
					select new BacklogItemReference
					{
						Id = b.Id,
						Name = b.Title,
						Type = b.Type
					}).ToListAsync();

				// Add resolved references
				foreach (var (fullId, linkType) in array)
				{
					var relatedTo = references.SingleOrDefault(r => r.Id == fullId);
					if (relatedTo == null)
						continue;
					existingRelatedItems.Add(new BacklogItemRelatedItem { LinkType = linkType, RelatedTo = relatedTo.RemoveEntityPrefixFromId() });
				}
			}
		}
	}
}
