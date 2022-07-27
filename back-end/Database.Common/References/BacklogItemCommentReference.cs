namespace Raven.Yabt.Database.Common.References;

public class BacklogItemCommentReference : IEntityReference
{
	/// <summary>
	///		The ID of the parented backlog item
	/// </summary>
	public string? Id { get; set; }

	/// <summary>
	///		The backlog item's title
	/// </summary>
	public string Name { get; set; } = null!; // Name is non-nullable

	/// <summary>
	///		The comment's ID
	/// </summary>
	public string? CommentId { get; set; }
}