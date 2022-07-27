#nullable disable  // Disable nullable check for a response DTO file
using System;
using System.Text.Json.Serialization;

namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs;

public class CommentResponse
{
	public uint Id { get; set; }

	[JsonPropertyName("created_at")]
	public DateTime Created { get; set; }
	[JsonPropertyName("updated_at")]
	public DateTime Updated { get; set; }
	public string Body { get; set; }
	public UserResponse User { get; set; }
}