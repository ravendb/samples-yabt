using System.Threading.Tasks;

using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery
{
	public interface IBacklogItemByIdQueryService
	{
		Task<BacklogItemGetResponse?> GetById(string id);
	}
}
