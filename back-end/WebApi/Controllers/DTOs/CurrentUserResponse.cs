using Raven.Yabt.Domain.UserServices.Query.DTOs;

namespace Raven.Yabt.WebApi.Controllers.DTOs;

public class CurrentUserResponse: UserGetByIdResponse
{
	public string Id { get; }

	public CurrentUserResponse(UserGetByIdResponse user, string id)
	{
		Id = id;
		FirstName = user.FirstName;
		LastName = user.LastName;
		FullName = user.FullName;
		NameWithInitials = user.NameWithInitials;
		AvatarUrl = user.AvatarUrl;
		Email = user.Email;
	}
}