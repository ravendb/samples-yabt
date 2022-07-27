using Raven.Yabt.Domain.UserServices.Common;

#nullable disable   // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.UserServices.Query.DTOs;

public class UserGetByIdResponse: IUserItemBaseDto
{
	public string FirstName { get; set; }
	public string LastName { get; set; }
	public string FullName { get; set; }
	public string NameWithInitials { get; set; }
	public string AvatarUrl { get; set; }
	public string Email { get; set; }
}