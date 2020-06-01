namespace Raven.Yabt.Database.Common.References
{
	/// <summary>
	///     Interface, representing key-value pair for entities
	/// </summary>
	public interface IEntityReference
	{
		/// <summary>
		///		The ID may be absent for deleted records. 
		///		E.g. a user has been removed from the DB, but tickets' history may want to keep records of the user's modifications
		/// </summary>
		string? Id { get; }
		string Name { get; }
	}
}