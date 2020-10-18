#nullable disable  // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.UserServices.Command.DTOs
{
	public class UserAddUpdRequest
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? AvatarUrl { get; set; }
		public string Email { get; set; }
	}
}
