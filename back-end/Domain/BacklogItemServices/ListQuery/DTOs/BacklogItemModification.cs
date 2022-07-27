namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;

public class BacklogItemModification
{
	public string UserId { get; set; } = null!;
	public ModificationType Type { get; set; } = ModificationType.Any;

	public enum ModificationType
	{
		Any,
		CreatedOnly
	}
}