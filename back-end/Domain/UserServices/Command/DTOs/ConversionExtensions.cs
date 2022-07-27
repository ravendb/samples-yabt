using Raven.Yabt.Database.Models.Users;

namespace Raven.Yabt.Domain.UserServices.Command.DTOs;

/// <summary>
///		Adapter design pattern in mapping DTOs instead of AutoMapper to enforce strong type checks (see more https://gigi.nullneuron.net/gigilabs/the-adapter-design-pattern-for-dtos-in-c/, https://cezarypiatek.github.io/post/why-i-dont-use-automapper/)
/// </summary>
internal static class ConversionExtensions
{
	public static User ConvertToUser(this UserAddUpdRequest dto, User? entity = null)
	{
		entity ??= new User();

		entity.FirstName = dto.FirstName;
		entity.LastName = dto.LastName;
		entity.NameWithInitials = (!string.IsNullOrEmpty(dto.FirstName) ? $"{dto.FirstName.Substring(0, 1)}. " : "") + dto.LastName ?? "";
		entity.AvatarUrl = dto.AvatarUrl;
		entity.Email = dto.Email;

		return entity;
	}
}