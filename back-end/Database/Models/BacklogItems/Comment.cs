using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public class Comment
	{
		[JsonIgnore]
		public string Id => $"{CreatedDate:O}";
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
		public DateTime ModifiedDate { get; set; } = DateTime.UtcNow;

		public UserReference Author { get; set; } = null!;	// Non-nullable
		public string Message { get; set; } = null!;		// Non-nullable

		/// <summary>
		/// 	Mentioned users in the <see cref="Message"/>, e.g. { 'HomerSimpson', 'users/2-A' }
		/// </summary>
		/// <remarks>
		/// 	It's nullable to avoid storing in the DB a field without any values
		/// </remarks>
		public IDictionary<string, string>? MentionedUserIds { get; set; }
	}
}
