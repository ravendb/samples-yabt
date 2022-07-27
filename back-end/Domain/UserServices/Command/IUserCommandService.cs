using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

namespace Raven.Yabt.Domain.UserServices.Command;

public interface IUserCommandService
{
	Task<IDomainResult<UserReference>> Create(UserAddUpdRequest dto);

	Task<IDomainResult<UserReference>> Update(string id, UserAddUpdRequest dto);

	Task<IDomainResult<UserReference>> Delete(string id);
}