using System.Text.Json.Serialization;

#nullable disable  // Disable nullable check for a response DTO file
namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs;

public class UserResponse
{
	public uint Id { get; set; }
	public string Login { get; set; }
	[JsonPropertyName("avatar_url")]
	public string AvatarUrl { get; set; }
	[JsonPropertyName("gravatar_id")]
	public string GravatarUrl { get; set; }
}