using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public class BacklogItemTask : BacklogItem
	{
		public override BacklogItemType Type { get; set; } = BacklogItemType.Task;
	}
}
