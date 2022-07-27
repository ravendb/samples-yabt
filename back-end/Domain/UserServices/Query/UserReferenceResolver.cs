using System;
using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.Domain.UserServices.Query;

public class UserReferenceResolver : BaseService<User>, IUserReferenceResolver
{
	private readonly ICurrentUserResolver _currentUserResolver;

	public UserReferenceResolver(IAsyncTenantedDocumentSession dbSession, ICurrentUserResolver currentUserResolver) : base(dbSession)
	{
		_currentUserResolver = currentUserResolver;
	}

	/// <inheritdoc/>
	public async Task<UserReference?> GetReferenceById(string id)
	{
		var fullId = GetFullId(id);

		var user = await DbSession.LoadAsync<User>(fullId);

		return user?.ToReference().RemoveEntityPrefixFromId();
	}

	/// <inheritdoc/>
	public async Task<UserReference> GetCurrentUserReference()
	{
		var currentUserId = _currentUserResolver.GetCurrentUserId();
		var userRef = await GetReferenceById(currentUserId);
		if (userRef == null)
			throw new ArgumentException($"Can't resolve user details for #{currentUserId}");
		return userRef.RemoveEntityPrefixFromId();
	}
}