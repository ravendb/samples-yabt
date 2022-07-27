namespace Raven.Yabt.Database.Common.References;

/// <summary>
///     Represents a key-value pair for referencing entities
/// </summary>
public abstract record EntityReferenceBase : IEntityReference
{
	/// <summary>
	///		The record ID. Can be NULL if the record has been deleted
	/// </summary>
	public string? Id { get; init; }

	/// <summary>
	///		Name/title used in the reference
	/// </summary>
	public string Name { get; set; } = null!;

	protected EntityReferenceBase(string? id, string name)
	{
		Id = id;
		Name = name;
	}
	protected EntityReferenceBase() {}

	public void Deconstruct(out string? id, out string name) => (id, name) = (Id, Name);
}