using System.Threading.Tasks;

using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command
{
	public interface ICustomFieldCommandService
	{
		Task<CustomFieldReferenceDto> Create(CustomFieldAddRequest dto);

		Task<CustomFieldReferenceDto?> Rename(string id, CustomFieldRenameRequest dto);

		Task<CustomFieldReferenceDto?> Delete(string id);
	}
}
