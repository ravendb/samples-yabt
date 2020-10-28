using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.UserServices.Query
{
	public interface IUserReferenceResolver
	{
		Task<UserReference?> GetReferenceById(string id);

		Task<UserReference> GetCurrentUserReference();
	}
}
