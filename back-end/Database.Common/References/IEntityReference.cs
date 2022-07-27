namespace Raven.Yabt.Database.Common.References;

/// <summary>
///     Interface, representing key-value pair for referencing entities
/// </summary>
public interface IEntityReference
{
	/// <summary>
	///     The ID field (reserved type and name)
	/// </summary>
	/// <remarks>
	///		Can be null for deleted records.
	///		E.g. a user has been removed from the DB, but tickets' history may want to keep records of the user's modifications
	/// </remarks>
	string? Id { get; }

	/// <summary>
	///		Shorten name/title used in the reference
	/// </summary>
	string Name { get; }
}