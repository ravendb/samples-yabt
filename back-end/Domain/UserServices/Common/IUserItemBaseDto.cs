namespace Raven.Yabt.Domain.UserServices.Common;

public interface IUserItemBaseDto
{
	public string? FirstName { get; }
	public string? LastName { get; }
	public string? AvatarUrl { get; }
	public string Email { get; }
}