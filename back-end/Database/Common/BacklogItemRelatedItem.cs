using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Common
{
	public class BacklogItemRelatedItem
	{
		public BacklogItemReference RelatedTo { get; set; } = null!;

		public BacklogRelationshipType LinkType { get; set; }
	}
}
