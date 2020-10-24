using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.BacklogItemServices.CommentQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentQuery
{
	public class BacklogItemCommentQueryService: BaseQueryService<BacklogItem>, IBacklogItemCommentQueryService
	{
		public BacklogItemCommentQueryService (IAsyncDocumentSession dbSession) : base(dbSession) {}
		
		public async Task<ListResponse<BacklogItemCommentListGetResponse>> GetList(string backlogItemId, BacklogItemCommentListGetRequest dto)
		{
			var fullId = GetFullId(backlogItemId);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return new ListResponse<BacklogItemCommentListGetResponse>();
			
			var commentQuery = ApplySorting(ticket.Comments, dto)
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

			return new ListResponse<BacklogItemCommentListGetResponse>(ret, ticket.Comments.Count, dto.PageIndex, dto.PageSize);
		}

		private IEnumerable<Comment> ApplySorting(IEnumerable<Comment> comments, BacklogItemCommentListGetRequest dto)
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
