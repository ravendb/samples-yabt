using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Query;

public interface ICustomFieldByIdQueryService
{
	Task<IDomainResult<CustomFieldItemResponse>> GetById(string id);
}