namespace Raven.Yabt.Database.Common.References
{
	public class BacklogItemReference : IEntityReference
	{
		public string? Id { get; set; }

		public string Name { get; set; } = null!; // Name is non-nullable
		
		public BacklogItemType Type { get; set; }
	}
}
