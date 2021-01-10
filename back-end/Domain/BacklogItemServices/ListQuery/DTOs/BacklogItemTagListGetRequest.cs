using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs
{
	public class BacklogItemTagListGetRequest : ISearchable
	{
		public string? Search { get; set; }
	}
}
