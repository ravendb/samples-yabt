using Raven.Yabt.Database.Models.Users;

namespace Raven.Yabt.Domain.UserServices.Query.DTOs;

/// <summary>
///		Adapter design pattern in mapping DTOs instead of AutoMapper to enforce strong type checks (see more https://gigi.nullneuron.net/gigilabs/the-adapter-design-pattern-for-dtos-in-c/, https://cezarypiatek.github.io/post/why-i-dont-use-automapper/)
/// </summary>
internal static class ConversionExtensions
{
	public static UserGetByIdResponse ConvertToUserDto(this User dto)
	{
		return new UserGetByIdResponse
		{
			FirstName = dto.FirstName,
			LastName = dto.LastName,
			FullName = dto.FullName,
			NameWithInitials = dto.NameWithInitials,
			AvatarUrl = dto.AvatarUrl,
			Email = dto.Email
		};
	}
}