namespace Raven.Yabt.Database.Common.References
{
	/// <summary>
	///		Reference to a <see cref="Models.User"/> record
	/// </summary>
	public class UserReference : IEntityReference
	{
		/// <inheritdoc/>
		public string? Id { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; } = null!;   // Non-nullable
		/// <summary>
		///		Full name of the user
		/// </summary>
		public string FullName { get; set; } = null!;   // Non-nullable

		/// <summary>
		///		Link to the avaatar
		/// </summary>
		public string? Avatar { get; set; }
	}
}
