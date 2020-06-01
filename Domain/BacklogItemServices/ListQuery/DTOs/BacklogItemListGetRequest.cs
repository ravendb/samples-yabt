using System.Collections.Generic;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs
{
	public class BacklogItemListGetRequest : ListRequest<BacklogItemsOrderColumns>
	{
		public BacklogItemType Type { get; set; } = BacklogItemType.Unknown;

		public string? Search { get; set; }

		public string? AssignedUserId { get; set; }

		public bool ModifiedByTheCurrentUserOnly { get; set; }

		public BacklogItemModification? UserModification { get; set; }

		public IDictionary<string,string>? CustomField { get; set; }
	}
}
