using System.Threading.Tasks;

using Raven.Yabt.Domain.BacklogItemServices.CommentQuery.DTOs;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentQuery
{
	public interface IBacklogItemCommentQueryService
	{
		Task<ListResponse<BacklogItemCommentListGetResponse>> GetList(string backlogItemId, BacklogItemCommentListGetRequest dto);
	}
}
