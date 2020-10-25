using System;
using System.Collections.Generic;

using Raven.Yabt.Database.Common.References;

#nullable disable	// Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public class BacklogItemCommentListGetResponse
	{
		public string Id { get; set; }
		public string Message { get; set; }

		public UserReference Author { get; set; }

		public DateTime Created { get; set; }
		public DateTime LastUpdated { get; set; }
		
		public IDictionary<string, string> MentionedUserIds { get; set; }
	}
}
