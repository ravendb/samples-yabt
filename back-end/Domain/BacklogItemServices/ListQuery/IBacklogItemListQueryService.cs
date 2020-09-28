using System.Collections.Generic;
using System.Threading.Tasks;

using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery
{
	public interface IBacklogItemListQueryService
	{
		Task<List<BacklogItemListGetResponse>> GetList(BacklogItemListGetRequest dto);
	}
}
