using System.Threading.Tasks;

using Raven.Yabt.Domain.UserServices.DTOs;

namespace Raven.Yabt.Domain.UserServices
{
	public interface IUserQueryService
	{
		Task<UserGetByIdResponse?> GetById(string id);
	}
}
