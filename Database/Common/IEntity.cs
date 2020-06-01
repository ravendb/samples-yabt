namespace Raven.Yabt.Database.Common
{
	/// <summary>
	///     Applies a constraint on the ID fields across the entities
	/// </summary>
	public interface IEntity
	{
		/// <summary>
		///     The ID field (reserved type and name)
		/// </summary>
		string Id { get; set; }
	}
}