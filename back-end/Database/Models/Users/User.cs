using System;
using System.Text.RegularExpressions;

using Newtonsoft.Json;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.Users
{
	public class User : BaseEntity
	{
		/// <summary>
		///		First name. One of the first/last names should be mandatory
		/// </summary>
		public string? FirstName { get; set; } = string.Empty;
		/// <summary>
		///		First name. One of the first/last names should be mandatory
		/// </summary>
		public string? LastName { get; set; } = string.Empty;
		/// <summary>
		///		Full name of the user, e.g. "Homer Simpson"
		/// </summary>
		[JsonIgnore]
		public string FullName => (!string.IsNullOrEmpty(FirstName) ? $"{FirstName} " : "") + LastName ?? "";
		/// <summary>
		///		User's name how it appears in the text as a user's mentioning, e.g. "HomerSimpson".
		/// 	Strip all [\r\n\t\f\v] characters
		/// </summary>
		[JsonIgnore]
		public string MentionedName => Regex.Replace((!string.IsNullOrEmpty(FirstName) ? $"{FirstName}" : "") + LastName ?? "", @"\s+", "");
		/// <summary>
		///		Shorten name of the user, e.g. "Simpson H."
		/// </summary>
		public string ShortName { get; set; } = string.Empty;

		public string? AvatarUrl { get; set; }
		public string? Email { get; set; }

		/// <summary>
		///		Date/time of the user's registration in the system
		/// </summary>
		public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

		public UserReference ToReference() => new UserReference
		{
			Id = Id,
			Name = ShortName,
			FullName = FullName,
			AvatarUrl = AvatarUrl
		};
	}
}
