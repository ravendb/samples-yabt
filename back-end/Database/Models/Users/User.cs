using System;

using Newtonsoft.Json;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models.Users;

public class User : BaseTenantedEntity
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
	public string FullName => (!string.IsNullOrEmpty(FirstName) ? $"{FirstName} " : "") + (LastName ?? "");
	/// <summary>
	///		Shorten name of the user, e.g. "Simpson H."
	/// </summary>
	public string NameWithInitials { get; set; } = string.Empty;

	public string? AvatarUrl { get; set; }
	public string? Email { get; set; }

	/// <summary>
	///		Date/time of the user's registration in the system
	/// </summary>
	public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

	public UserReference ToReference() => new (Id, NameWithInitials, FullName, AvatarUrl);
}