using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Database.Models.BacklogItems;

public class BacklogItemUserStory : BacklogItem
{
	public string? AcceptanceCriteria { get; set; }

	public override BacklogItemType Type { get; protected set; } = BacklogItemType.UserStory;
}