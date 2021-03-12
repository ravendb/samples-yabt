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

			return DomainResult.Success(
									ticket.ToReference().RemoveEntityPrefixFromId()
								);
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

			return DomainResult.Success(
									entity.ToReference().RemoveEntityPrefixFromId()
								);
		}

		public async Task<IDomainResult<BacklogItemReference>> Delete(string id)
		{
			var ticket = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemReference>();

			DbSession.Delete(ticket);

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

				backlogItem.Assignee = userRef;
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
				var verifiedCustomFieldIds = await _customFieldQueryService.GetFullIdsOfExistingItems(
						dto.CustomFields.Where(pair => pair.Value != null).Select(pair => pair.Key)
					);
				entity.CustomFields = verifiedCustomFieldIds.ToDictionary(x => x.Value, x => dto.CustomFields[x.Key]!);
			}
			else
				entity.CustomFields = null;

			entity.RelatedItems = await ResolveChangedRelatedItems(entity.RelatedItems, dto.ChangedRelatedItems);

			// entity.CustomProperties = dto.CustomProperties;	TODO: De-serialise custom properties

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

		private async Task<IList<BacklogItemRelatedItem>?> ResolveChangedRelatedItems(IList<BacklogItemRelatedItem>? existingRelatedItems, IList<BacklogRelationshipAction>? actions)
		{
			if (actions == null)
				return existingRelatedItems;

			// Remove 'old' links
			foreach (var (id, linkType) in from a in actions
												where a.ActionType == BacklogRelationshipActionType.Remove
												select (GetFullId(a.BacklogItemId), a.RelationType))
			{
				var itemToRemove = existingRelatedItems?.FirstOrDefault(existing => existing.RelatedTo.Id == id && existing.LinkType == linkType);
				if (itemToRemove != null) existingRelatedItems?.Remove(itemToRemove);
			}
			
			// Add new links
			(string fullId, BacklogRelationshipType linkType)[] array = 
				(from a in actions
				 where a.ActionType == BacklogRelationshipActionType.Add
				 select (GetFullId(a.BacklogItemId), a.RelationType)
				 ).ToArray();
			if (array.Any())
			{
				existingRelatedItems ??= new List<BacklogItemRelatedItem>();
				
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
					existingRelatedItems.Add(new BacklogItemRelatedItem { LinkType = linkType, RelatedTo = relatedTo });
				}
			}
			return existingRelatedItems;
		}
	}
}
