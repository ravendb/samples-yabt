namespace Raven.Yabt.Domain.CustomFieldServices.Command;

public interface IRemoveCustomFieldReferencesCommand
{
	void ClearCustomFieldId(string customFieldId);
}