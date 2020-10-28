using System.Collections.Generic;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public abstract class BacklogItemGetResponseBase
	{
		public string Title { get; set; } = null!;
		public BacklogItemType Type { get; set; }

		public ChangedByUserReference Created { get; set; } = null!;
		public ChangedByUserReference LastUpdated { get; set; } = null!;

		public string[] Tags { get; set; } = null!;
		public ListResponse<BacklogItemCommentListGetResponse>? Comments { get; set; }

		public IDictionary<string, object>? CustomFields { get; set; }
	}
}