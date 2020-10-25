using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery
{
	public class BacklogItemByIdQueryService : BaseService<BacklogItem>, IBacklogItemByIdQueryService
	{
		public BacklogItemByIdQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		/// <inheritdoc/>
		public async Task<IDomainResult<BacklogItemGetResponseBase>> GetById(string id, BacklogItemCommentListGetRequest? @params=null)
		{
			var fullId = GetFullId(id);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemGetResponseBase>();

			var comments = GetCommentsList(ticket, @params);

			var dto = (ticket.Type) switch
			{
				BacklogItemType.Bug			=> (ticket as BacklogItemBug)		?.ConvertToDto<BacklogItemBug, BugGetResponse>(comments) as  BacklogItemGetResponseBase,
				BacklogItemType.UserStory	=> (ticket as BacklogItemUserStory)	?.ConvertToDto<BacklogItemUserStory, UserStoryGetResponse>(comments) as BacklogItemGetResponseBase,
				_ => throw new NotImplementedException($"Not supported Backlog Item Type: {ticket.Type}"),
			};
			if (dto == null)
				throw new NotSupportedException($"Failed to return Backlog Item type of {ticket.Type}");

			return DomainResult.Success(dto);
		}
		
		/// <inheritdoc/>
		public async Task<ListResponse<BacklogItemCommentListGetResponse>> GetBacklogItemComments(string backlogItemId, BacklogItemCommentListGetRequest @params)
		{
			var fullId = GetFullId(backlogItemId);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return new ListResponse<BacklogItemCommentListGetResponse>();
			
			var ret = GetCommentsList(ticket, @params) ?? new List<BacklogItemCommentListGetResponse>();

			return new ListResponse<BacklogItemCommentListGetResponse>(ret, ticket.Comments.Count, @params.PageIndex, @params.PageSize);
		}

		private List<BacklogItemCommentListGetResponse>? GetCommentsList(BacklogItem backlogEntity, BacklogItemCommentListGetRequest? dto)
		{
			dto ??= new BacklogItemCommentListGetRequest();
			if (dto.PageSize == 0)
				return null;
			
			var commentQuery = ApplyCommentsSorting(backlogEntity.Comments, dto)
			                   .Skip(dto.PageIndex * dto.PageSize)
			                   .Take(dto.PageSize);
			var ret = (from comment in commentQuery
				select new BacklogItemCommentListGetResponse
				{
					Id = comment.Id,
					Message = comment.Message,
					Author = comment.Author,
					Created = comment.CreatedDate,
					LastUpdated = comment.ModifiedDate,
					MentionedUserIds = comment.MentionedUserIds
				}).ToList();
			ret.RemoveEntityPrefixFromIds(r => r.Author);
			return ret;
		}

		private IEnumerable<Comment> ApplyCommentsSorting(IEnumerable<Comment> comments, BacklogItemCommentListGetRequest dto)
		{
			if (dto.OrderBy == BacklogItemCommentsOrderColumns.Default)
			{
				dto.OrderBy = BacklogItemCommentsOrderColumns.TimestampLastModified;
				dto.OrderDirection = OrderDirections.Desc;
			}

			return dto.OrderBy switch
			{
				BacklogItemCommentsOrderColumns.TimestampLastModified =>	
					dto.OrderDirection == OrderDirections.Asc 
						? comments.OrderBy(t => t.ModifiedDate) 
						: comments.OrderByDescending(t => t.ModifiedDate),
				_ => throw new NotImplementedException()
			};
		}
	}
}
