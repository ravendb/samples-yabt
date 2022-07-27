namespace Raven.Yabt.Database.Models;

/// <summary>
///		Token interfaces for the entity that defines tenants/projects in the system
/// </summary>
public interface ITenant
{
	/// <summary>
	///     The ID field
	/// </summary>
	string Id { get; }
}