using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

public abstract record BacklogItemAddUpdRequestBase
{
	/// <summary>
	///		The ticket's title
	/// </summary>
	[Required]
	public string Title { get; set; } = null!;

	public BacklogItemState State { get; set; }
		
	/// <summary>
	///		Estimated size of the item in the configured units (e.g. 'days')
	/// </summary>
	public uint? EstimatedSize { get; set; }

	public string? AssigneeId { get; set; }

	/// <summary>
	///		Tags. Note that each tag can't exceed 10 characters
	/// </summary>
	public string[]? Tags { get; set; }

	/// <summary>
	///		Changed related tickets: { Backlog Item ID, Relationship type, Action type: add/remove }.
	/// </summary>
	public IList<BacklogRelationshipAction>? ChangedRelatedItems { get; set; }

	/// <summary>
	///		Changed custom properties of various data types configured by the user: { Custom Field ID, Value, Action type: add/remove }.
	/// </summary>
	public IList<BacklogCustomFieldAction>? ChangedCustomFields { get; set; }
}