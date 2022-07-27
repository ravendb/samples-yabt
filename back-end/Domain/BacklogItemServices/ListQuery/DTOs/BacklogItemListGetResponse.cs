using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.Common;

#nullable disable	// Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs
{
	public class BacklogItemListGetResponse: ListResponseWithSanitisedIds
	{
		public string Title { get; set; }

		public BacklogItemType Type { get; set; }
		
		public BacklogItemState State { get; set; }
		
		public string[] Tags { get; set; }

		public UserReference Assignee { get; set; }
		
		public int CommentsCount { get; set; }

		public ChangedByUserReference Created { get; set; }
		public ChangedByUserReference LastUpdated { get; set; }
	}
}
