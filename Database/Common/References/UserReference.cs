namespace Raven.Yabt.Database.Common.References
{
	public class UserReference : IEntityReference
	{
		public string? Id { get; set; }

		public string Name { get; set; } = null!;   // Non-nullable
		public string FullName { get; set; } = null!;   // Non-nullable

		public string? Avatar { get; set; }
	}
}
