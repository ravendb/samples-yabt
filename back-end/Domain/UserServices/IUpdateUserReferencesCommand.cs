using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.UserServices
{
	/// <summary>
	///		Handles updates and deletes of users.
	///		Multiple instances of the interface are supported.
	/// </summary>
	public interface IUpdateUserReferencesCommand : IPatchUpdateNotificationHandler
	{
		void UpdateReferences(UserReference newUserReference);
		void ClearUserId(string userId);
	}
}
