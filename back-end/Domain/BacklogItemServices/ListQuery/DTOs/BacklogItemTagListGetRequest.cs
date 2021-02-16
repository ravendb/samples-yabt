using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs
{
	public class BacklogItemTagListGetRequest : ISearchable
	{
		/// <inheritdoc/>
		public string? Search { get; set; }
	}
}
