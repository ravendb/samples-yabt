﻿using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public class BacklogItemBug : BacklogItem
	{
		// Severity and priority of the bug, where enums define the available options
		public BugSeverity? Severity { get; set; }
		public BugPriority? Priority { get; set; }

		public string? StepsToReproduce { get; set; }
		public string? AcceptanceCriteria { get; set; }

		public override BacklogItemType Type { get; set; } = BacklogItemType.Bug;
	}
}
