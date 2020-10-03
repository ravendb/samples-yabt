using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.UserServices.DTOs;

namespace Raven.Yabt.Domain.UserServices
{
	public interface IUserQueryService
	{
		Task<IDomainResult<UserGetByIdResponse>> GetById(string id);
	}
}
