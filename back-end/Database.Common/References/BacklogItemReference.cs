using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Database.Common.References;

public record BacklogItemReference : EntityReferenceBase
{
	public BacklogItemType Type { get; init; }
		
	public BacklogItemReference() {}
	public BacklogItemReference(string? id, string name, BacklogItemType type) : base(id, name)
	{
		Type = type;
	}
}