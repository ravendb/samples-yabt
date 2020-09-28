using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItem;
using Raven.Yabt.Database.Models.BacklogItem.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	public class BacklogItemCommandService : BaseService<BacklogItem>, IBacklogItemCommandService
	{
		private readonly IUserReferenceResolver _userResolver;
		private readonly ICustomFieldQueryService _customFieldQueryService;

		public BacklogItemCommandService(IAsyncDocumentSession dbSession, IUserReferenceResolver userResolver, ICustomFieldQueryService customFieldQueryService) : base(dbSession)
		{
			_userResolver = userResolver;
			_customFieldQueryService = customFieldQueryService;
		}

		public async Task<BacklogItemReference> Create<T>(T dto) where T : BacklogItemAddUpdRequest
		{
			BacklogItem ticket = dto switch
			{
				BugAddUpdRequest bug		 => await ConvertDtoToEntity<BacklogItemBug, BugAddUpdRequest>(bug),
				UserStoryAddUpdRequest story => await ConvertDtoToEntity<BacklogItemUserStory, UserStoryAddUpdRequest>(story),
				_ => throw new ArgumentException("Incorrect type", nameof(dto)),
			};

			await DbSession.StoreAsync(ticket);

			return ticket.ToReference().RemoveEntityPrefixFromId();
		}

		public async Task<BacklogItemReference?> Delete(string id)
		{
			var ticket = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
			if (ticket == null)
				return null;

			DbSession.Delete(ticket);

			return ticket.ToReference().RemoveEntityPrefixFromId();
		}

		public async Task<BacklogItemReference?> Update<T>(string id, T dto) where T : BacklogItemAddUpdRequest
		{
			if (dto == null)
				throw new ArgumentNullException(nameof(dto));

			var entity = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
			if (entity == null)
				return null;

			entity = dto switch
			{
				BugAddUpdRequest bug			=> await ConvertDtoToEntity (bug,	entity as BacklogItemBug),
				UserStoryAddUpdRequest story	=> await ConvertDtoToEntity (story,	entity as BacklogItemUserStory),
				_ => throw new ArgumentException("Incorrect type", nameof(dto)),
			};

			return entity.ToReference().RemoveEntityPrefixFromId();
		}

		public async Task<BacklogItemReference?> AssignToUser(string backlogItemId, string userShortenId)
		{
			var backlogItem = await DbSession.LoadAsync<BacklogItem>(GetFullId(backlogItemId));
			if (backlogItem == null)
				return null;

			if (userShortenId == null)
			{
				backlogItem.Assignee = null;
			}
			else
			{
				var userRef = await _userResolver.GetReferenceById(userShortenId);
				if (userRef == null)
					return null;

				backlogItem.Assignee = userRef;
			}

			backlogItem.ModifiedBy.Add(new BacklogItemHistoryRecord
				{
					ActionedBy = await _userResolver.GetCurrentUserReference(),
					Summary = "Assigned user"
				});

			return backlogItem.ToReference().RemoveEntityPrefixFromId();
		}

		private async Task<TModel> ConvertDtoToEntity<TModel, TDto>(TDto dto, TModel? entity = null)
			where TModel : BacklogItem, new()
			where TDto : BacklogItemAddUpdRequest
		{
			if (entity == null)
				entity = new TModel();

			entity.Title = dto.Title;
			entity.Assignee = dto.AssigneeId != null ? await _userResolver.GetReferenceById(dto.AssigneeId) : null;
			entity.ModifiedBy.Add(new BacklogItemHistoryRecord
				{
					ActionedBy = await _userResolver.GetCurrentUserReference(),
					Summary = entity.ModifiedBy?.Any() == true ? "Modified" : "Created"
				});

			if (dto.CustomFields != null)
			{
				var verifiedCustomFieldIds = await _customFieldQueryService.GetFullIdsOfExistingItems(dto.CustomFields.Keys);
				entity.CustomFields = verifiedCustomFieldIds.ToDictionary(x => x.Value, x => dto.CustomFields[x.Key]);
			}
			else
				entity.CustomFields.Clear();

			if (dto.RelatedItems != null)
				entity.RelatedItems = await ResolveRelatedItems(dto.RelatedItems);
			else
				entity.RelatedItems.Clear();

			// entity.CustomProperties = dto.CustomProperties;	TODO: De-serialise custom properties

			if (dto is BugAddUpdRequest bugDto && entity is BacklogItemBug bugEntity)
			{
				bugEntity.Severity = bugDto.Severity;
				bugEntity.Priority = bugDto.Priority;
				bugEntity.StepsToReproduce = bugDto.StepsToReproduce;
			}
			else if (dto is UserStoryAddUpdRequest storyDto && entity is BacklogItemUserStory storyEntity)
			{
				storyEntity.AcceptanceCriteria = storyDto.AcceptanceCriteria;
			}

			return entity;
		}

		private async Task<IList<BacklogItemRelatedItem>> ResolveRelatedItems(IDictionary<string, BacklogRelationshipType>? relatedItems)
		{
			if (relatedItems == null)
				return new List<BacklogItemRelatedItem>();

			var ids = relatedItems.Keys.Select(GetFullId);

			var references = await (from b in DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
									where b.Id.In(ids)
									select new BacklogItemReference
									{
										Id = b.Id,
										Name = b.Title,
										Type = b.Type
									}).ToListAsync();

			return (from r in references
					select new BacklogItemRelatedItem
					{
						RelatedTo = r,
						LinkType = relatedItems[r.Id!]
					}).ToList();
		}
	}
}
