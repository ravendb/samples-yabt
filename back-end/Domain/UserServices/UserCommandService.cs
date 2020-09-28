using System.Collections.Generic;
using System.Threading.Tasks;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.DTOs;

namespace Raven.Yabt.Domain.UserServices
{
	public class UserCommandService : BaseService<User>, IUserCommandService
	{
		private readonly IEnumerable<IUpdateUserReferencesCommand> _updateUserReferences;

		public UserCommandService(IAsyncDocumentSession dbSession, IEnumerable<IUpdateUserReferencesCommand> updateUserReferences) : base(dbSession) 
		{
			_updateUserReferences = updateUserReferences;
		}

		public async Task<UserReference> Create(UserAddUpdRequest dto)
		{
			var user = dto.ConvertToUser();
			await DbSession.StoreAsync(user);

			return user.ToReference().RemoveEntityPrefixFromId();
		}

		public async Task<UserReference?> Delete(string id)
		{
			var fullId = GetFullId(id);

			var user = await DbSession.LoadAsync<User>(fullId);
			if (user == null)
				return null;

			DbSession.Delete(fullId);

			// Delete the ID in all refrences to this user
			foreach (var updateUserRef in _updateUserReferences)
				updateUserRef.ClearUserId(id);

			return user.ToReference().RemoveEntityPrefixFromId();
		}

		public async Task<UserReference?> Update(string id, UserAddUpdRequest dto)
		{
			var fullId = GetFullId(id);

			var user = await DbSession.LoadAsync<User>(fullId);
			if (user == null)
				return null;

			var newRef = dto.ConvertToUser(user).ToReference();

			// Update the name in all refrences to this user
			foreach (var updateUserRef in _updateUserReferences)
				updateUserRef.UpdateReferences(newRef);

			return user.ToReference().RemoveEntityPrefixFromId();
		}
	}
}
