using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery
{
	public interface IBacklogItemByIdQueryService
	{
		/// <summary>
		/// 	Get a Backlog Item by its ID
		/// </summary>
		/// <param name="id"> The ID of the backlog item </param>
		/// <param name="params"> [OPTIONAL] Parameters for fetching comments </param>
		/// <returns> The backlog item </returns>
		Task<IDomainResult<BacklogItemGetResponseBase>> GetById(string id, BacklogItemCommentListGetRequest? @params=null);

		/// <summary>
		/// 	Get paginated comments of a Backlog Item
		/// </summary>
		/// <param name="backlogItemId"> The ID of the backlog item </param>
		/// <param name="params"> Parameters for fetching comments </param>
		/// <returns> Paginated list of comments </returns>
		Task<ListResponse<BacklogItemCommentListGetResponse>> GetBacklogItemComments(string backlogItemId, BacklogItemCommentListGetRequest @params);
	}
}
