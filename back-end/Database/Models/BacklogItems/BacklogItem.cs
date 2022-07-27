using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
// ReSharper disable RedundantCast

namespace Raven.Yabt.Database.Models.BacklogItems;

/// <summary>
///		Base class representing common properties across all types of tickets: bugs, user stories, etc.
/// </summary>
/// <remarks>
///		Can't make the class 'abstract', due to getting exception: Cannot find collection name for abstract class, only concrete class are supported. 
/// </remarks>
public class BacklogItem : BaseTenantedEntity
{
	/// <summary>
	///		The Title [mandatory field]
	/// </summary>
	public string Title { get; set; } = null!;

	public virtual BacklogItemType Type { get; protected set; }		// Can't make it 'abstract'

	/// <summary>
	///		Current state of the backlog item
	/// </summary>
	public BacklogItemState State { get; set; } = BacklogItemState.New;
		
	/// <summary>
	///		Estimated size of the item in the configured units (e.g. 'days')
	/// </summary>
	public uint? EstimatedSize { get; set; }
		
	/// <summary>
	///		The assigned user to the ticket
	/// </summary>
	public UserReference? Assignee { get; set; }

	/// <summary>
	///		List of all users who modified the ticket.
	///		The first record is creation of the ticket
	/// </summary>
	public List<BacklogItemHistoryRecord> ModifiedBy { get; } = new();

	[JsonIgnore]
	public ChangedByUserReference Created		=> ModifiedBy.OrderBy(m => m.Timestamp).First() as ChangedByUserReference;
	[JsonIgnore]
	public ChangedByUserReference LastUpdated	=> ModifiedBy.OrderBy(m => m.Timestamp).Last() as ChangedByUserReference;

	/// <summary>
	///		Tags/Labels on the ticket
	/// </summary>
	public string[]? Tags { get; set; }

	/// <summary>
	///		Related tickets
	/// </summary>
	/// <remarks>
	///		Having an empty array by default simplifies processing in the code - no need to account for NULL
	/// </remarks>
	public List<BacklogItemRelatedItem> RelatedItems { get; } = new ();

	/// <summary>
	///		Comments on the ticket
	/// </summary>
	/// <remarks>
	///		Having an empty array by default simplifies processing in the code - no need to account for NULL
	/// </remarks>
	public IList<Comment> Comments { get; } = new List<Comment>();

	/// <summary>
	///		Extra custom properties of various data types configured by the user,
	///		Stored as { custom field ID, value }
	/// </summary>
	public IDictionary<string, object>? CustomFields { get; set; }

	/// <summary>
	/// 	Add a record about modification of the entity
	/// </summary>
	public void AddHistoryRecord(UserReference actionedBy, string message)
	{
		ModifiedBy.Add(new BacklogItemHistoryRecord(actionedBy, message));
			
		// Cap the number of records in 100 most recent one (an arbitrary number to avoid the collection getting out of proportion)
		// Note: we keep the first record to avoid losing the date of creation
		const int maxCount = 100;
		if (ModifiedBy.Count > maxCount)
		{
			var orderedList = ModifiedBy.OrderByDescending(m => m.Timestamp).ToList();
			var firstTimestamp = orderedList.Last().Timestamp; 
			var lastTimestamp = orderedList.Skip(maxCount - 1).First().Timestamp;
			ModifiedBy.RemoveAll(m => firstTimestamp < m.Timestamp && m.Timestamp < lastTimestamp );
		}
	}

	public BacklogItemReference ToReference() 
		=> new()
		{
			Id = Id,
			Name = Title,
			Type = Type
		};
}