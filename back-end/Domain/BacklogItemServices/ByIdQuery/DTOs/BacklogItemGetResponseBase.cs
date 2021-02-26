using System.Collections.Generic;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public abstract class BacklogItemGetResponseBase
	{
		public string Title { get; set; } = null!;
		public BacklogItemType Type { get; set; }
		public BacklogItemState State { get; set; }
		
		/// <summary>
		///		Estimated size of the item in the configured units (e.g. 'days')
		/// </summary>
		public uint? EstimatedSize { get; set; }
		
		public UserReference? Assignee { get; set; }

		public ChangedByUserReference Created { get; set; } = null!;
		public ChangedByUserReference LastUpdated { get; set; } = null!;

		public string[]? Tags { get; set; }
		public IList<BacklogItemCommentListGetResponse>? Comments { get; set; }

		/// <summary>
		///		Extra custom properties of various data types configured by the user: { Custom Field ID, Value }.
		/// </summary>
		public IDictionary<string, object>? CustomFields { get; set; }
		
		/// <summary>
		///		Related tickets
		/// </summary>
		public IList<BacklogItemRelatedItem>?  RelatedItems { get; set; }
	}
}