using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public class BacklogItemRelatedItem
	{
		public BacklogItemReference RelatedTo { get; set; } = null!;

		public BacklogRelationshipType LinkType { get; set; }
	}
}
