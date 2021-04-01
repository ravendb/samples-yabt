using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.Query;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	public class BacklogItemCommandService : BaseService<BacklogItem>, IBacklogItemCommandService
	{
		private readonly IUserReferenceResolver _userResolver;
		private readonly IBacklogItemDtoToEntityConversion _dtoToEntityConversion;

		public BacklogItemCommandService(IAsyncDocumentSession dbSession, IUserReferenceResolver userResolver, IBacklogItemDtoToEntityConversion dtoToEntityConversion) : base(dbSession)
		{
			_userResolver = userResolver;
			_dtoToEntityConversion = dtoToEntityConversion;
		}

		public async Task<IDomainResult<BacklogItemReference>> Create<T>(T dto) where T : BacklogItemAddUpdRequestBase
		{
			BacklogItem? ticket = dto switch
			{
				BugAddUpdRequest bug		 => await _dtoToEntityConversion.ConvertToEntity<BacklogItemBug,		BugAddUpdRequest>(bug),
				UserStoryAddUpdRequest story => await _dtoToEntityConversion.ConvertToEntity<BacklogItemUserStory,	UserStoryAddUpdRequest>(story),
				TaskAddUpdRequest task		 => await _dtoToEntityConversion.ConvertToEntity<BacklogItemTask,		TaskAddUpdRequest>(task),
				FeatureAddUpdRequest feature => await _dtoToEntityConversion.ConvertToEntity<BacklogItemFeature,	FeatureAddUpdRequest>(feature),
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
				BugAddUpdRequest bug		 => await _dtoToEntityConversion.ConvertToEntity (bug,		entity as BacklogItemBug),
				UserStoryAddUpdRequest story => await _dtoToEntityConversion.ConvertToEntity (story,	entity as BacklogItemUserStory),
				TaskAddUpdRequest task		 => await _dtoToEntityConversion.ConvertToEntity (task,		entity as BacklogItemTask),
				FeatureAddUpdRequest feature => await _dtoToEntityConversion.ConvertToEntity (feature,	entity as BacklogItemFeature),
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
		
		private void UpdateRelatedItems<T>(T dto, BacklogItemReference ticketRef) where T : BacklogItemAddUpdRequestBase
		{
			if (dto.ChangedRelatedItems?.Any() != true) return;
			
			foreach (var link in dto.ChangedRelatedItems)
			{
				if (link.ActionType == ListActionType.Add)
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
	}
}
