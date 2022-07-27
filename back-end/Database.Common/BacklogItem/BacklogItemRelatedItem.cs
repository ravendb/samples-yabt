using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Common.BacklogItem;

public record BacklogItemRelatedItem
{
	public BacklogItemReference RelatedTo { get; set; } = null!;

	public BacklogRelationshipType LinkType { get; set; }
}