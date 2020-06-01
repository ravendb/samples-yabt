using System.Threading.Tasks;

using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command
{
	public interface ICustomFieldCommandService
	{
		Task<CustomFieldReference> Create(CustomFieldAddRequest dto);

		Task<CustomFieldReference?> Rename(string id, CustomFieldRenameRequest dto);

		Task<CustomFieldReference?> Delete(string id);
	}
}
