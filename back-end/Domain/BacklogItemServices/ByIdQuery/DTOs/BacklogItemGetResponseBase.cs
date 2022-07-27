using System.Collections.Generic;
using System.Linq;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

public abstract class BacklogItemGetResponseBase
{
	public string Title { get; init; } = null!;
	public BacklogItemType Type { get; init; }
	public BacklogItemState State { get; init; }
		
	/// <summary>
	///		Estimated size of the item in the configured units (e.g. 'days')
	/// </summary>
	public uint? EstimatedSize { get; init; }
		
	public UserReference? Assignee { get; init; }

	/// <summary>
	///		List of all users who modified the ticket.
	///		The last record is creation of the ticket
	/// </summary>
	public IReadOnlyList<BacklogItemHistoryRecord> HistoryDescOrder { get; init; }
		
	public ChangedByUserReference Created => HistoryDescOrder.Last();
	public ChangedByUserReference LastUpdated => HistoryDescOrder.First();

	public string[]? Tags { get; init; }
	public IReadOnlyList<BacklogItemCommentListGetResponse>? Comments { get; init; }

	/// <summary>
	///		Extra custom properties of various data types configured by the user
	/// </summary>
	public IReadOnlyList<BacklogItemCustomFieldValue>? CustomFields { get; init; }
		
	/// <summary>
	///		Related tickets
	/// </summary>
	public IReadOnlyList<BacklogItemRelatedItem>?  RelatedItems { get; init; }
}