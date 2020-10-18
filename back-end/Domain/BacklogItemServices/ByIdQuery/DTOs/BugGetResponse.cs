using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public class BugGetResponse : BacklogItemGetResponseBase
	{
		public BugSeverity? Severity { get; set; }
		public BugPriority? Priority { get; set; }

		public string? StepsToReproduce { get; set; }
	}
}
