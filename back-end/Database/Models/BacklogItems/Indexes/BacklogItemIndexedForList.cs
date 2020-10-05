using System;
using System.Collections.Generic;

using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Database.Models.BacklogItems.Indexes
{
	public class BacklogItemIndexedForList : BacklogItem, ISearchable
	{
		public override BacklogItemType Type { get; set; }

		public string? AssignedUserId { get; set; }

		public string? CreatedByUserId { get; set; }
		public DateTime CreatedTimestamp { get; set; }
		public DateTime LastUpdatedTimestamp { get; set; }

		public BugSeverity? Severity { get; set; }
		public BugPriority? Priority { get; set; }

		public string Search { get; set; } = null!;

		/// <summary>
		///		Ticket modifications: { User ID, Timestamp of a change }.
		///		Used for searching changes by a user.
		/// </summary>
		public IDictionary<string, DateTime> ModifiedByUser { get; set; } = null!;

		/// <summary>
		///		Custom fields: { Custom Field ID, Value }.
		///		Change the data type from the model to trick eliminate C# value comparison checks as it's handled on the Raven's side
		/// </summary>
		public new IDictionary<string, object> CustomFields { get; set; } = null!;
	}
}
