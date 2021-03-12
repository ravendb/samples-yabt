using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs
{
	public abstract record BacklogItemAddUpdRequestBase
	{
		/// <summary>
		///		The ticket's title
		/// </summary>
		[Required]
		public string Title { get; set; }

		public BacklogItemState State { get; set; }
		
		/// <summary>
		///		Estimated size of the item in the configured units (e.g. 'days')
		/// </summary>
		public uint? EstimatedSize { get; set; }

		public string? AssigneeId { get; set; }

		public string[]? Tags { get; set; }

		/// <summary>
		///		Related tickets: { Backlog Item ID, Relationship type, Action type: add/remove }.
		/// </summary>
		public IList<BacklogRelationshipAction>? ChangedRelatedItems { get; set; }

		/// <summary>
		///		Extra custom properties of various data types configured by the user: { Custom Field ID, Value }.
		/// </summary>
		public IDictionary<string, object?>? CustomFields { get; set; }
	}
}
