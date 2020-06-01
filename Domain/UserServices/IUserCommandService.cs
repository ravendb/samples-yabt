using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.UserServices.DTOs;

namespace Raven.Yabt.Domain.UserServices
{
	public interface IUserCommandService
	{
		Task<UserReference> Create(UserAddUpdRequest dto);

		Task<UserReference?> Update(string id, UserAddUpdRequest dto);

		Task<UserReference?> Delete(string id);
	}
}
