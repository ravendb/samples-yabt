using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Database.Models.BacklogItem
{
	public class BacklogItemUserStory : BacklogItem
	{
		public string? AcceptanceCriteria { get; set; }

		public override BacklogItemType Type { get; set; } = BacklogItemType.UserStory;
	}
}
