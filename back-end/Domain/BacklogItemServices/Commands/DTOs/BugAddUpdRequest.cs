using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs
{
	public class BugAddUpdRequest : BacklogItemAddUpdRequestBase
	{
		public BugSeverity? Severity { get; set; }
		public BugPriority? Priority { get; set; }

		public string? StepsToReproduce { get; set; }
		public string? AcceptanceCriteria { get; set; }
	}
}
