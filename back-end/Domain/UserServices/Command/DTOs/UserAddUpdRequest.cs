namespace Raven.Yabt.Domain.UserServices.Command.DTOs
{
	public class UserAddUpdRequest
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? AvatarUrl { get; set; }
		public string Email { get; set; } = null!;
	}
}
