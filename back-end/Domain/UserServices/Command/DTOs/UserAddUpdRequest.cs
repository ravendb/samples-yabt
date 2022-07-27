using Raven.Yabt.Domain.UserServices.Common;

namespace Raven.Yabt.Domain.UserServices.Command.DTOs;

public class UserAddUpdRequest : IUserItemBaseDto
{
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? AvatarUrl { get; set; }
	public string Email { get; set; } = null!;
}