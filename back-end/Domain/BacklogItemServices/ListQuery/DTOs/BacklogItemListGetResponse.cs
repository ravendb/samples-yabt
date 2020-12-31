using System.Linq;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;

#nullable disable	// Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs
{
	public class BacklogItemListGetResponse
	{
		private string _id;

		public string Id 
		{
			get { return _id;  }
			set { _id = value?.Split('/').Last(); } 
		}
		public string Title { get; set; }

		public BacklogItemType Type { get; set; }
		
		public BacklogItemState State { get; set; }
		
		public string[] Tags { get; set; }

		public UserReference Assignee { get; set; }

		public ChangedByUserReference Created { get; set; }
		public ChangedByUserReference LastUpdated { get; set; }
	}
}
