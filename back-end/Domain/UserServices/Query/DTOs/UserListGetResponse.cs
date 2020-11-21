using System;
using System.Linq;

namespace Raven.Yabt.Domain.UserServices.Query.DTOs
{
	public class UserListGetResponse
	{
		private string? _id;
		public string? Id
		{
			get => _id;
			set => _id = value?.Split('/').Last();
		}

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
}
