namespace Raven.Yabt.Database.Models;

/// <summary>
///     Applies a constraint on the ID fields
/// </summary>
public interface IEntity
{
	/// <summary>
	///     The ID field (reserved type and name)
	/// </summary>
	string Id { get; }
}