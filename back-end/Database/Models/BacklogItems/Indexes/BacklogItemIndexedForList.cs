using System;
using System.Collections.Generic;

using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Database.Models.BacklogItems.Indexes;

public class BacklogItemIndexedForList : BacklogItem, ISearchable
{
	public string? AssignedUserId { get; set; }
	public string AssignedUserName { get; set; } = string.Empty;

	public string? CreatedByUserId { get; set; }
	public DateTime CreatedTimestamp { get; set; }
	public DateTime LastUpdatedTimestamp { get; set; }

	public BugSeverity? Severity { get; set; }
	public BugPriority? Priority { get; set; }

	public string Search { get; set; } = null!;

	/// <summary>
	/// 	Mentioned users: { User ID, Timestamp of the corresponding comment }.
	/// </summary>
	public IDictionary<string, DateTime>? MentionedUser { get; set; }
		
	/// <summary>
	///		Ticket modifications: { User ID, Timestamp of a change }.
	///		Used for searching changes by a user.
	/// </summary>
	public IDictionary<string, DateTime> ModifiedByUser { get; set; } = null!;
}