using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.UserServices.DTOs;

namespace Raven.Yabt.Domain.UserServices
{
	public class UserQueryService : BaseService<User>, IUserQueryService
	{
		public UserQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		public async Task<IDomainResult<UserGetByIdResponse>> GetById(string id)
		{
			var fullId = GetFullId(id);

			var user = await DbSession.LoadAsync<User>(fullId);
			if (user == null)
				return DomainResult.NotFound<UserGetByIdResponse>();

			return DomainResult.Success(user.ConvertToUserDto());
		}
	}
}
