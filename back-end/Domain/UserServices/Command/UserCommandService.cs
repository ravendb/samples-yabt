using System.Collections.Generic;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

namespace Raven.Yabt.Domain.UserServices.Command;

public class UserCommandService : BaseService<User>, IUserCommandService
{
	private readonly IEnumerable<IUpdateUserReferencesCommand> _updateUserReferences;

	public UserCommandService(IAsyncTenantedDocumentSession dbSession, IEnumerable<IUpdateUserReferencesCommand> updateUserReferences) : base(dbSession)
	{
		_updateUserReferences = updateUserReferences;
	}

	public async Task<IDomainResult<UserReference>> Create(UserAddUpdRequest dto)
	{
		var user = dto.ConvertToUser();
		await DbSession.StoreAsync(user);

		var response = user.ToReference().RemoveEntityPrefixFromId();

		return DomainResult.Success(response);
	}

	public async Task<IDomainResult<UserReference>> Delete(string id)
	{
		var fullId = GetFullId(id);

		var user = await DbSession.LoadAsync<User>(fullId);
		if (user == null)
			return DomainResult.NotFound<UserReference>();

		DbSession.Delete(user);

		// Delete the ID in all references to this user
		foreach (var updateUserRef in _updateUserReferences)
			updateUserRef.ClearUserId(id);

		var response = user.ToReference().RemoveEntityPrefixFromId();

		return DomainResult.Success(response);
	}

	public async Task<IDomainResult<UserReference>> Update(string id, UserAddUpdRequest dto)
	{
		var fullId = GetFullId(id);

		var user = await DbSession.LoadAsync<User>(fullId);
		if (user == null)
			return DomainResult.NotFound<UserReference>();

		var newRef = dto.ConvertToUser(user).ToReference().RemoveEntityPrefixFromId();

		// Update the name in all references to this user
		foreach (var updateUserRef in _updateUserReferences)
			updateUserRef.UpdateReferences(newRef);

		return DomainResult.Success(newRef);
	}
}