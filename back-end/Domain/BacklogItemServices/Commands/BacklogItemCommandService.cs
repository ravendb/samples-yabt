using System;
using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.Query;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands;

public class BacklogItemCommandService : BaseService<BacklogItem>, IBacklogItemCommandService
{
	private readonly IUserReferenceResolver _userResolver;
	private readonly IBacklogItemDtoToEntityConversion _dtoToEntityConversion;

	public BacklogItemCommandService(IAsyncTenantedDocumentSession dbSession, IUserReferenceResolver userResolver, IBacklogItemDtoToEntityConversion dtoToEntityConversion) : base(dbSession)
	{
		_userResolver = userResolver;
		_dtoToEntityConversion = dtoToEntityConversion;
	}

	public async Task<IDomainResult<BacklogItemReference>> Create<T>(T dto) where T : BacklogItemAddUpdRequestBase
	{
		var (ticket, status) = dto switch
		{
			BugAddUpdRequest bug		 => await _dtoToEntityConversion.ConvertToEntity<BacklogItemBug,		BugAddUpdRequest>(bug),
			UserStoryAddUpdRequest story => await _dtoToEntityConversion.ConvertToEntity<BacklogItemUserStory,	UserStoryAddUpdRequest>(story),
			TaskAddUpdRequest task		 => await _dtoToEntityConversion.ConvertToEntity<BacklogItemTask,		TaskAddUpdRequest>(task),
			FeatureAddUpdRequest feature => await _dtoToEntityConversion.ConvertToEntity<BacklogItemFeature,	FeatureAddUpdRequest>(feature),
			_ => throw new ArgumentException($"Unsupported type ${typeof(T)}", nameof(dto))
		};
		if (!status.IsSuccess)
			return status.To<BacklogItemReference>();

		await DbSession.StoreAsync(ticket);
		var ticketRef = ticket.ToReference().RemoveEntityPrefixFromId();

		await UpdateRelatedItems(dto, ticketRef);
			
		return DomainResult.Success(ticketRef);
	}

	public async Task<IDomainResult<BacklogItemReference>> Update<T>(string id, T dto) where T : BacklogItemAddUpdRequestBase
	{
		var entity = await DbSession.LoadAsync<BacklogItem>(GetFullId(id));
		if (entity == null)
			return DomainResult.NotFound<BacklogItemReference>();

		var (_, status) = dto switch
		{
			BugAddUpdRequest bug		 => await _dtoToEntityConversion.ConvertToEntity (bug,		entity as BacklogItemBug),
			UserStoryAddUpdRequest story => await _dtoToEntityConversion.ConvertToEntity (story,	entity as BacklogItemUserStory),
			TaskAddUpdRequest task		 => await _dtoToEntityConversion.ConvertToEntity (task,		entity as BacklogItemTask),
			FeatureAddUpdRequest feature => await _dtoToEntityConversion.ConvertToEntity (feature,	entity as BacklogItemFeature),
			_ => throw new ArgumentException($"Unsupported type ${typeof(T)}", nameof(dto))
		};
		if (!status.IsSuccess)
			return status.To<BacklogItemReference>();

		var ticketRef = entity.ToReference().RemoveEntityPrefixFromId();

		await UpdateRelatedItems(dto, ticketRef);
			
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
				DbSession.PatchWithoutValidation<BacklogItem, BacklogItemRelatedItem>(item.RelatedTo.Id!,
					x => x.RelatedItems,
					items => items.RemoveAll(i => i.RelatedTo.Id == shortId));
			}

		return DomainResult.Success(
				ticket.ToReference().RemoveEntityPrefixFromId()
			);
	}

	public async Task<IDomainResult> SetState(string backlogItemId, BacklogItemState newState)
	{
		if (!await DbSession.Patch<BacklogItem, BacklogItemState>(backlogItemId, x => x.State, newState))
			return IDomainResult.NotFound();

		await AddHistoryRecordPatch(backlogItemId, $"Changed status to {newState.ToString()}");
			
		return IDomainResult.Success();
	}

	public async Task<IDomainResult> AssignToUser(string backlogItemId, string? userShortenId)
	{
		UserReference? userRef = null;
		if (userShortenId != null)
		{
			userRef = await _userResolver.GetReferenceById(userShortenId);
			if (userRef == null)
				return DomainResult.NotFound("The user not found");
		}

		if (!await DbSession.Patch<BacklogItem, UserReference?>(backlogItemId, x => x.Assignee, userRef))
			return IDomainResult.NotFound("The Backlog Item not found");
			
		await AddHistoryRecordPatch(backlogItemId, userRef == null ? "Removed assigned user" : $"Assigned user '{userRef.MentionedName}'");
			
		return IDomainResult.Success();
	}
		
	private async Task UpdateRelatedItems<T>(T dto, BacklogItemReference ticketRef) where T : BacklogItemAddUpdRequestBase
	{
		if (dto.ChangedRelatedItems?.Any() != true) return;
			
		foreach (var link in dto.ChangedRelatedItems)
		{
			if (link.ActionType == ListActionType.Add)
			{
				DbSession.PatchWithoutValidation<BacklogItem, BacklogItemRelatedItem>(
					link.BacklogItemId,
					x => x.RelatedItems,
					items => items.Add(
						new BacklogItemRelatedItem
						{
							RelatedTo = ticketRef,
							LinkType = link.RelationType.GetMirroredType()
						}));
					
				await AddHistoryRecordPatch(link.BacklogItemId, $"Added related item {ticketRef.Id}");
			}
			else
			{
				var relatedId = ticketRef.Id!;
				var relationType = link.RelationType.GetMirroredType();
				DbSession.PatchWithoutValidation<BacklogItem, BacklogItemRelatedItem>(
					link.BacklogItemId,
					x => x.RelatedItems,
					items => items.RemoveAll(
						i => i.RelatedTo.Id! == relatedId && i.LinkType == relationType));
					
				await AddHistoryRecordPatch(link.BacklogItemId, $"Removed related item {ticketRef.Id}");
			}
		}
	}

	private async Task AddHistoryRecordPatch(string backlogItemId, string message)
	{
		var userRef = await _userResolver.GetCurrentUserReference();
		DbSession.PatchWithoutValidation<BacklogItem, BacklogItemHistoryRecord>(
			backlogItemId,
			x => x.ModifiedBy,
			items => items.Add(new BacklogItemHistoryRecord(userRef, message)));
	}
}