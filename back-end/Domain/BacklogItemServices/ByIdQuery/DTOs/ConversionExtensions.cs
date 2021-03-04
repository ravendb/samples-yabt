using System.Collections.Generic;
using System.Linq;

using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.Common;
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
				Assignee = entity.Assignee,
				Created = entity.Created,
				LastUpdated = entity.LastUpdated,
				Tags = entity.Tags,
				Comments = GetCommentsList(entity),
				RelatedItems = entity.RelatedItems,
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

			return response.RemoveEntityPrefixFromIds(r => r.Created.ActionedBy, r => r.LastUpdated.ActionedBy, r => r.Assignee);
		}
		
		private static List<BacklogItemCommentListGetResponse>? GetCommentsList(BacklogItem backlogEntity)
		{
			var ret = (from comment in backlogEntity.Comments.OrderByDescending(c => c.Created)
				select new BacklogItemCommentListGetResponse
				{
					Id = comment.Id,
					Message = comment.Message,
					Author = comment.Author,
					Created = comment.Created,
					LastUpdated = comment.LastModified,
					MentionedUserIds = comment.MentionedUserIds?.ToDictionary(pair => pair.Key, pair => pair.Value.GetShortId()!)
				}).ToList();
			ret.RemoveEntityPrefixFromIds(r => r.Author);
			return ret;
		}
	}
}
