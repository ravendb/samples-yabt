using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.UserServices.Command;

/// <summary>
///		Handles updates and deletes of users.
///		Multiple instances of the interface are supported.
/// </summary>
public interface IUpdateUserReferencesCommand
{
	void UpdateReferences(UserReference newUserReference);
	void ClearUserId(string userId);
}