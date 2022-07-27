namespace Raven.Yabt.Database.Models.BacklogItems.Indexes;

public class BacklogItemTagsIndexed : ITenantedEntity
{
	public string Name { get; init; } = null!;
	public int Count { get; init; }
	public string TenantId { get; init; } = null!;
}