using System.Text.RegularExpressions;

using Raven.Client;
using Raven.Client.Documents.Queries;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models;
using Raven.Yabt.Database.Models.BacklogItem;
using Raven.Yabt.Database.Models.BacklogItem.Indexes;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	internal class UpdateUserReferencesCommand : IUpdateUserReferencesCommand
	{
		private readonly IPatchOperationsExecuteAsync _patchOperations;

		public UpdateUserReferencesCommand(IPatchOperationsExecuteAsync patchOperations)
		{
			_patchOperations = patchOperations;
		}

		public void ClearUserId(string userId)
		{
			// Replace invalid characters with empty strings. Can't pass it as a parameter, as string parameters get quated when inserted
			var idForDynamicField = Regex.Replace(userId, @"[^\w\.@-]", "").GetIdForDynamicField<User>();
			var fullId = userId.GetFullId<User>();

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.ModifiedBy)}_{idForDynamicField} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.ModifiedBy)}.forEach(modif => {{
																			if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)}.toLowerCase() == '{fullId}'.toLowerCase())
																				modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)} = null;
																		}});
								}}";
			var query = new IndexQuery { Query = queryString };

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}

		public void UpdateReferences(UserReference newUserReference)
		{
			// Replace invalid characters with empty strings. Can't pass it as a parameter, as string parameters get quated when inserted
			var idForDynamicField = Regex.Replace(newUserReference.Id, @"[^\w\.@-]", "");

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.ModifiedBy)}_{idForDynamicField} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.ModifiedBy)}.forEach(modif => {{
																			if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)}.toLowerCase() == $userId.toLowerCase())
																				modif.{nameof(BacklogItemHistoryRecord.ActionedBy)} = $userRef;
																		}});
								}}";
			var query = new IndexQuery
			{
				Query = queryString,
				QueryParameters = new Parameters
				{
					{ "userId", newUserReference.Id },
					{ "userRef", newUserReference },
				}
			};

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}
	}
}
