using System;
using System.Collections.Generic;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.BacklogItem
{
	public class Comment
	{
		public Guid Id { get; set; }
		public DateTime CreatedDate { get; set; }

		public UserReference Author { get; set; } = null!;  // Non-nullable
		public string Message { get; set; } = null!;    // Non-nullable

		public IList<string> MentionedUserIds { get; } = new List<string>();
	}
}
