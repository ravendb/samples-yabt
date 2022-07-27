using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command;

public interface ICustomFieldCommandService
{
	Task<IDomainResult<CustomFieldReferenceDto>> Create(CustomFieldAddRequest dto);

	Task<IDomainResult<CustomFieldReferenceDto>> Update(string id, CustomFieldUpdateRequest dto);

	Task<IDomainResult<CustomFieldReferenceDto>> Delete(string id);
}