using System;
using System.Collections.Generic;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.BacklogItems
{
	public class Comment
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

		public UserReference Author { get; set; } = null!;  // Non-nullable
		public string Message { get; set; } = null!;    // Non-nullable

		public IList<string> MentionedUserIds { get; } = new List<string>();
	}
}
