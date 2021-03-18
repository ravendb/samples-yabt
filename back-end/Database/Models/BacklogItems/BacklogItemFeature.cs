using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public class BacklogItemFeature : BacklogItem
	{
		public override BacklogItemType Type { get; set; } = BacklogItemType.Feature;
	}
}
