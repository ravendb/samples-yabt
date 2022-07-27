using System;

using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.UserServices.Query.DTOs;

public class UserListGetResponse: ListResponseWithSanitisedIds
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
	public string FullName => $"{FirstName} {LastName}";
	/// <summary>
	///		Shorten name of the user, e.g. "Simpson H."
	/// </summary>
	public string NameWithInitials { get; set; } = string.Empty;

	public string? Email { get; set; }

	/// <summary>
	///		Date/time of the user's registration in the system
	/// </summary>
	public DateTime RegistrationDate { get; set; }
}