using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command
{
	public interface ICustomFieldCommandService
	{
		Task<IDomainResult<CustomFieldReferenceDto>> Create(CustomFieldAddRequest dto);

		Task<IDomainResult<CustomFieldReferenceDto>> Rename(string id, CustomFieldRenameRequest dto);

		Task<IDomainResult<CustomFieldReferenceDto>> Delete(string id);
	}
}
