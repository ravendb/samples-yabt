using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.UserServices.Query;

public interface IUserReferenceResolver
{
	/// <summary>
	///		Get a user reference for a user identified by an ID.
	/// </summary>
	/// <remarks>
	///		The ID doesn't include the 'users/' prefix  
	/// </remarks>
	Task<UserReference?> GetReferenceById(string id);

	/// <summary>
	///		Get a user reference for the current user.
	/// </summary>
	/// <remarks>
	///		The ID doesn't include the 'users/' prefix  
	/// </remarks>
	Task<UserReference> GetCurrentUserReference();
}