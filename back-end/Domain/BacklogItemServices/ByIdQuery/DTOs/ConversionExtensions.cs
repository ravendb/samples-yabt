using System.Collections.Generic;

using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	/// <summary>
	///		Adapter design pattern in mapping DTOs instead of AutoMapper to enforce strong type checks (see more https://gigi.nullneuron.net/gigilabs/the-adapter-design-pattern-for-dtos-in-c/, https://cezarypiatek.github.io/post/why-i-dont-use-automapper/)
	/// </summary>
	internal static class ConversionExtensions
	{
		public static TResponse ConvertToDto<TEntity, TResponse>(this TEntity entity, List<BacklogItemCommentListGetResponse>? comments)
			where TEntity : BacklogItem
			where TResponse : BacklogItemGetResponseBase, new()
		{
			var response = new TResponse
			{
				Id = entity.Id,
				Title = entity.Title,
				Created = entity.Created,
				LastUpdated = entity.LastUpdated,
				Tags = entity.Tags,
				Comments = comments,
				CustomFields = entity.CustomFields,
				Type = entity.Type
			};
			response.RemoveEntityPrefixFromIds(r => r.Created.ActionedBy, r => r.LastUpdated.ActionedBy);

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
	}
}
