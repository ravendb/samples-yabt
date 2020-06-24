using Newtonsoft.Json;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Database.Models
{
	public class User : IEntity
	{
		/// <summary>
		///		The record ID
		/// </summary>
		/// <remarks>
		///		Set by Raven Client. Can be temporarily null before passed to the DocumentSession.Store() method
		/// </remarks>
		public string Id { get; set; } = null!;

		/// <summary>
		///		First name. One of the first/last names should be mandatory
		/// </summary>
		public string FirstName { get; set; } = string.Empty;
		/// <summary>
		///		First name. One of the first/last names should be mandatory
		/// </summary>
		public string LastName { get; set; } = string.Empty;
		[JsonIgnore]
		public string FullName => $"{FirstName} {LastName}";
		public string ShortName { get; set; } = string.Empty;

		public string? Avatar { get; set; }
		public string? Email { get; set; }

		public UserReference ToReference() => new UserReference
		{
			Id = Id,
			Name = ShortName,
			FullName = FullName,
			Avatar = Avatar
		};
	}
}
