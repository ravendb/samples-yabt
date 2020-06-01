using System;

namespace Raven.Yabt.Database.Common.References
{
	public class ChangedByUserReference
	{
		/// <summary>
		///		Timestamp of the change
		/// </summary>
		public DateTime Timestamp { get; set; } = DateTime.UtcNow;

		/// <summary>
		///		The user who made the change
		/// </summary>
		public UserReference ActionedBy { get; set; } = null!;	// Non-nullable
	}
}
