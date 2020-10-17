using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery
{
	public interface IBacklogItemByIdQueryService
	{
		Task<IDomainResult<BacklogItemGetResponseBase>> GetById(string id);
	}
}
