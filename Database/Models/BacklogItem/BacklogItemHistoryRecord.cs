using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.BacklogItem
{
	public class BacklogItemHistoryRecord : ChangedByUserReference
	{
		/// <summary>
		///		Brief summary of the change
		/// </summary>
		public string Summary { get; set; } = string.Empty;
	}
}
