using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;

public interface IBacklogItemByIdQueryService
{
	/// <summary>
	/// 	Get a Backlog Item by its ID
	/// </summary>
	/// <param name="id"> The ID of the backlog item </param>
	/// <returns> The backlog item </returns>
	Task<IDomainResult<BacklogItemGetResponseBase>> GetById(string id);
}