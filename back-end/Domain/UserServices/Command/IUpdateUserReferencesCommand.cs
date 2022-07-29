using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.UserServices.Command;

/// <summary>
///		Handles updates and deletes of users.
///		Multiple instances of the interface are supported.
/// </summary>
public interface IUpdateUserReferencesCommand
{
	/// <summary>
	///		Update the user's name on all the references
	/// </summary>
	/// <param name="newUserReference"> The reference for the updated user's name (the ID won't change) </param>
	void UpdateReferences(UserReference newUserReference);
	
	/// <summary>
	///		Clear all user references for the removed user 
	/// </summary>
	/// <param name="userId"> The ID of the removed user </param>
	void ClearUserId(string userId);
}