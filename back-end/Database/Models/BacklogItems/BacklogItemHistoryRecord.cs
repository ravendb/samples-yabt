using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public record BacklogItemHistoryRecord : ChangedByUserReference
	{
		/// <summary>
		///		Brief summary of the change
		/// </summary>
		public string? Summary { get; init; }
		
		public BacklogItemHistoryRecord() {}

		public BacklogItemHistoryRecord(UserReference actionedBy, string? summary = null) : base(actionedBy)
		{
			Summary = summary;
		}
	}
}
