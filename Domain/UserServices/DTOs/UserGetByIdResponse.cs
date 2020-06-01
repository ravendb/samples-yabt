#nullable disable   // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.UserServices.DTOs
{
	public class UserGetByIdResponse
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string FullName { get; set; }
		public string ShortName { get; set; }
		public string Avatar { get; set; }
		public string Email { get; set; }
	}
}
