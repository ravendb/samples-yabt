namespace Raven.Yabt.Database.Models;

/// <summary>
///     Entity that belongs to a tenant/project
/// </summary>
public interface ITenantedEntity
{
	/// <summary>
	///     ID of a tenant/project
	/// </summary>
	string TenantId { get; }
}