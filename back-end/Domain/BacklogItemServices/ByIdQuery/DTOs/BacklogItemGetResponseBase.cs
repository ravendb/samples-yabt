using System.Collections.Generic;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;

#nullable disable  // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public abstract class BacklogItemGetResponseBase
	{
		protected BacklogItemGetResponseBase() { }

		public string Id { get; set; }
		public string Title { get; set; }
		public BacklogItemType Type { get; set; }

		public ChangedByUserReference Created { get; set; }
		public ChangedByUserReference LastUpdated { get; set; }

		public string[] Tags { get; set; }
		public IList<BacklogItemCommentGetResponse> Comments { get; set; }

		public IDictionary<string, object> CustomFields { get; set; }
	}
}