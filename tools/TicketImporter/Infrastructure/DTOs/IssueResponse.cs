#nullable disable  // Disable nullable check for a response DTO file
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs
{
	public class IssueResponse
	{
		public uint Id { get; set; }
		public uint Number { get; set; }
		[JsonPropertyName("created_at")]
		public DateTime Created { get; set; }
		[JsonPropertyName("updated_at")]
		public DateTime Updated { get; set; }
		public string State { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		[JsonPropertyName("comments")]
		public int CommentsCount { get; set; }
		[JsonPropertyName("comments_url")]
		public string CommentsUrl { get; set; }
		[JsonIgnore]
		public List<CommentResponse> Comments { get; set; }
		public UserResponse User { get; set; }
		public UserResponse Assignee { get; set; }
		public LabelResponse[] Labels { get; set; }
	}
}
