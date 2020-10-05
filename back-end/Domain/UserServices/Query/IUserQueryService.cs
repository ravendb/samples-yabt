using System.Collections.Generic;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Domain.UserServices.Query.DTOs;

namespace Raven.Yabt.Domain.UserServices.Query
{
	public interface IUserQueryService
	{
		Task<IDomainResult<UserGetByIdResponse>> GetById(string id);
		Task<List<UserListGetResponse>> GetList(UserListGetRequest dto);
	}
}
