using System.Text.RegularExpressions;

using Raven.Client;
using Raven.Client.Documents.Queries;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItem;
using Raven.Yabt.Database.Models.BacklogItem.Indexes;
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
			// Replace invalid characters with empty strings.
			userId = Regex.Replace(userId, @"[^\w\.@-]", "");

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.Modifications)}_M{userId} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.Modifications)}.forEach(modif => {{
																			if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)} == '{userId}')
																				modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)} = null;
																		}});
								}}";
			var query = new IndexQuery { Query = queryString };

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}

		public void UpdateReferences(UserReference newUserReference)
		{
			// Replace invalid characters with empty strings.
			newUserReference.Id = Regex.Replace(newUserReference.Id, @"[^\w\.@-]", "");

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.Modifications)}_M{newUserReference.Id} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.Modifications)}.forEach(modif => {{
																			if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)} == $userId)
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
