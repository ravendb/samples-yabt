#nullable disable  // Disable nullable check for a response DTO file
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs;

public class IssueResponse
{
	public uint Id { get; init; }
	public uint Number { get; init; }
		
	[JsonPropertyName("created_at")]
	public DateTime Created { get; init; }
	[JsonPropertyName("updated_at")]
	public DateTime Updated { get; init; }
		
	public string State { get; init; }
	public string Title { get; init; }
	public string Body { get; init; }
	[JsonPropertyName("comments")]
	public int CommentsCount { get; init; }
	[JsonPropertyName("comments_url")]
	public string CommentsUrl { get; init; }
	[JsonIgnore]
	public List<CommentResponse> Comments { get; set; }
	public UserResponse User { get; init; }
	public UserResponse Assignee { get; init; }
	public LabelResponse[] Labels { get; init; }

	[JsonPropertyName("pull_request")]
	public IDictionary<string,string> PullRequests { get; init; }

	[JsonIgnore]
	public bool IsPullRequest => PullRequests?.Any() == true;
}