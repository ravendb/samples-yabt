namespace Raven.Yabt.Database.Models;

public class BaseTenantedEntity : BaseEntity, ITenantedEntity
{
	public string TenantId { get; init; } = null!;
}