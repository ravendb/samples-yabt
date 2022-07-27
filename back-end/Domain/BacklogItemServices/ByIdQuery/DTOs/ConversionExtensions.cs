using System.Collections.Generic;
using System.Linq;

using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

/// <summary>
///		Adapter design pattern in mapping DTOs instead of AutoMapper to enforce strong type checks (see more https://gigi.nullneuron.net/gigilabs/the-adapter-design-pattern-for-dtos-in-c/, https://cezarypiatek.github.io/post/why-i-dont-use-automapper/)
/// </summary>
internal static class ConversionExtensions
{
	public static TResponse ConvertToDto<TEntity, TResponse>(this TEntity entity, List<BacklogItemCustomFieldValue>? customFieldValues)
		where TEntity : BacklogItem
		where TResponse : BacklogItemGetResponseBase, new()
	{
		var response = new TResponse
			{
				Title = entity.Title,
				State = entity.State,
				EstimatedSize = entity.EstimatedSize,
				Assignee = entity.Assignee is null ? null : (entity.Assignee with {}).RemoveEntityPrefixFromId(),
				HistoryDescOrder = entity.ModifiedBy.OrderByDescending(i => i.Timestamp).ToList(),
				Tags = entity.Tags,
				Comments = GetCommentsList(entity.Comments),
				RelatedItems = entity.RelatedItems.AsReadOnly(),
				CustomFields = customFieldValues?.AsReadOnly(),
				Type = entity.Type
			};

		switch (entity)
		{
			case BacklogItemBug entityBug when response is BugGetResponse responseBug:
				responseBug.Priority = entityBug.Priority;
				responseBug.Severity = entityBug.Severity;
				responseBug.StepsToReproduce = entityBug.StepsToReproduce;
				break;
			case BacklogItemUserStory entityUserStory when response is UserStoryGetResponse responseUserStory:
				responseUserStory.AcceptanceCriteria = entityUserStory.AcceptanceCriteria;
				break;
			case BacklogItemTask entityTask when response is TaskGetResponse responseTask:
				responseTask.Description = entityTask.Description;
				break;
			case BacklogItemFeature entityFeature when response is FeatureGetResponse responseFeature:
				responseFeature.Description = entityFeature.Description;
				break;
		}

		return response;
	}

	public static List<BacklogItemCustomFieldValue> ConvertFieldToDto(this IEnumerable<CustomFieldListGetResponse> customFieldList, IDictionary<string, object>? values)
		=> customFieldList.Select(f => new BacklogItemCustomFieldValue
		{
			CustomFieldId = f.Id!,
			Type = f.FieldType,
			IsMandatory = f.IsMandatory,
			Name = f.Name,
			Value = values![f.Id!]
		}).ToList();
		
	private static IReadOnlyList<BacklogItemCommentListGetResponse> GetCommentsList(IList<Comment> comments)
	{
		return (from comment in comments.OrderByDescending(c => c.Created)
			select new BacklogItemCommentListGetResponse
			{
				Id = comment.Id,
				Message = comment.Message,
				Author = comment.Author,
				Created = comment.Created,
				LastUpdated = comment.LastModified,
				MentionedUserIds = comment.MentionedUserIds?.ToDictionary(pair => pair.Key, pair => pair.Value.GetShortId()!)
			}).ToList().AsReadOnly();
	}
}