using System.Collections.Generic;
using System.Linq;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	/// <summary>
	///		Adapter design pattern in mapping DTOs instead of AutoMapper to enforce strong type checks (see more https://gigi.nullneuron.net/gigilabs/the-adapter-design-pattern-for-dtos-in-c/, https://cezarypiatek.github.io/post/why-i-dont-use-automapper/)
	/// </summary>
	internal static class ConversionExtensions
	{
		public static TResponse ConvertToDto<TEntity, TResponse>(this TEntity entity)
			where TEntity : BacklogItem
			where TResponse : BacklogItemGetResponseBase, new()
		{
			var response = new TResponse
			{
				Title = entity.Title,
				State = entity.State,
				EstimatedSize = entity.EstimatedSize,
				Assignee = entity.Assignee is null ? null : (entity.Assignee with {}).RemoveEntityPrefixFromId(),
				HistoryDescOrder = GetModifiedBy(entity.ModifiedBy),
				Tags = entity.Tags,
				Comments = GetCommentsList(entity.Comments),
				RelatedItems = GetRelatedItems(entity.RelatedItems),
				CustomFields = entity.CustomFields,
				Type = entity.Type
			};

			if (entity is BacklogItemBug entityBug 
				&& response is BugGetResponse responseBug)
			{
				responseBug.Priority = entityBug.Priority;
				responseBug.Severity = entityBug.Severity;
				responseBug.StepsToReproduce = entityBug.StepsToReproduce;
			}
			else if (entity is BacklogItemUserStory entityUserStory
				  && response is UserStoryGetResponse responseUserStory)
			{
				responseUserStory.AcceptanceCriteria = entityUserStory.AcceptanceCriteria;
			}

			return response;
		}

		private static IReadOnlyList<BacklogItemHistoryRecord> GetModifiedBy(IList<BacklogItemHistoryRecord> modifiedBy)
		{
			var ret = (from item in modifiedBy.OrderByDescending(i => i.Timestamp) select item with {}).ToList();
			ret.RemoveEntityPrefixFromIds(i => i.ActionedBy);
			return ret.AsReadOnly();
		}
		
		private static IReadOnlyList<BacklogItemCommentListGetResponse> GetCommentsList(IList<Comment> comments)
		{
			return (from comment in comments.OrderByDescending(c => c.Created)
				select new BacklogItemCommentListGetResponse
				{
					Id = comment.Id,
					Message = comment.Message,
					Author = (comment.Author with {}).RemoveEntityPrefixFromId(),
					Created = comment.Created,
					LastUpdated = comment.LastModified,
					MentionedUserIds = comment.MentionedUserIds?.ToDictionary(pair => pair.Key, pair => pair.Value.GetShortId()!)
				}).ToList().AsReadOnly();
		}
		
		private static IReadOnlyList<BacklogItemRelatedItem>? GetRelatedItems(IList<BacklogItemRelatedItem>? relatedItems)
		{
			if (relatedItems == null)
				return null;
			
			var ret = (from item in relatedItems.OrderByDescending(i => i.LinkType) select item with {}).ToList();
			ret.RemoveEntityPrefixFromIds(i => i.RelatedTo);
			return ret.AsReadOnly();
		}
	}
}
