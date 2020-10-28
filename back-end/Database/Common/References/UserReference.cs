using System.Text.RegularExpressions;

using NewtonsoftJson = Newtonsoft.Json;
using TextJson = System.Text.Json.Serialization;

namespace Raven.Yabt.Database.Common.References
{
	/// <summary>
	///		Reference to a <see cref="Models.Users.User"/> record
	/// </summary>
	public class UserReference : IEntityReference
	{
		/// <inheritdoc/>
		public string? Id { get; set; }

		/// <inheritdoc/>
		public string Name { get; set; } = null!;   // Non-nullable
		/// <summary>
		///		Full name of the user
		/// </summary>
		public string FullName { get; set; } = null!;   // Non-nullable

		/// <summary>
		///		Link to the avatar
		/// </summary>
		public string? AvatarUrl { get; set; }
		
		/// <summary>
		///		User's name how it appears in the text as a user's mentioning, e.g. "HomerSimpson".
		/// 	Strip all [\r\n\t\f\v] characters
		/// </summary>
		[TextJson.JsonIgnore]			// Ignore in the controller output
		[NewtonsoftJson.JsonIgnore]		// Ignore in the RavenDB storage 
		public string MentionedName => Regex.Replace(FullName, @"\s+", "");
	}
}
