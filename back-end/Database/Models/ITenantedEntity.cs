namespace Raven.Yabt.Database.Models
{
	/// <summary>
	///     Top-level entities, which belong to a tenant/project
	/// </summary>
	public interface ITenantedEntity
	{
		/// <summary>
		///     ID of a tenant/project
		/// </summary>
		string TenantId { get; }
	}
}