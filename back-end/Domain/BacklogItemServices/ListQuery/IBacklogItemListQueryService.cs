using System.Threading.Tasks;

using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery;

public interface IBacklogItemListQueryService
{
	Task<ListResponse<BacklogItemListGetResponse>> GetList(BacklogItemListGetRequest dto);
		
	Task<BacklogItemTagListGetResponse[]> GetTags(BacklogItemTagListGetRequest dto);
}