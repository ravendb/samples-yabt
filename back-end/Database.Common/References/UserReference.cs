using System.Text.RegularExpressions;

using NewtonsoftJson = Newtonsoft.Json;
using TextJson = System.Text.Json.Serialization;

namespace Raven.Yabt.Database.Common.References;

/// <summary>
///		Reference to a User record
/// </summary>
public record UserReference : EntityReferenceBase
{
	/// <summary>
	///		Full name of the user
	/// </summary>
	public string FullName { get; set; } = null!;

	/// <summary>
	///		Link to the avatar
	/// </summary>
	public string? AvatarUrl { get; init; }
		
	/// <summary>
	///		User's name how it appears in the text as a user's mentioning, e.g. "HomerSimpson".
	/// 	Strip all [\r\n\t\f\v] characters
	/// </summary>
	[TextJson.JsonIgnore]			// Ignore in the controller output
	[NewtonsoftJson.JsonIgnore]		// Ignore in the RavenDB storage 
	public string MentionedName => Regex.Replace(FullName, @"\s+", "");

	public UserReference() {}
	public UserReference(string? id, string name, string fullName, string? avatarUrl = null) : base(id, name)
	{
		FullName = fullName;
		AvatarUrl = avatarUrl;
	}
}