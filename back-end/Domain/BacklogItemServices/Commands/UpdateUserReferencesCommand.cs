using System.Text.RegularExpressions;

using Raven.Client;
using Raven.Client.Documents.Queries;
using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	internal class UpdateUserReferencesCommand : IUpdateUserReferencesCommand
	{
		private readonly IPatchOperationsAddDeferred _patchOperations;

		public UpdateUserReferencesCommand(IPatchOperationsAddDeferred patchOperations)
		{
			_patchOperations = patchOperations;
		}

		public void ClearUserId(string userId)
		{
			if (string.IsNullOrEmpty(userId))
				return;

			var sanitisedId = GetSanitisedId(userId);

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.ModifiedBy)}_{sanitisedId} != null OR i.{nameof(BacklogItemIndexedForList.AssignedUserId)} == $userId
								UPDATE
								{{
									if (i.{nameof(BacklogItemIndexedForList.Assignee)}.{nameof(UserReference.Id)}.toLowerCase() == $userId) {{
										i.{nameof(BacklogItemIndexedForList.Assignee)} = null;
									}}
									i.{nameof(BacklogItem.Comments)}.forEach(comment => {{
										if (comment.{nameof(Comment.Author)}.{nameof(UserReference.Id)}.toLowerCase() == $userId) {{
											comment.{nameof(Comment.Author)}.{nameof(UserReference.Id)} = null;
										}}
									}});
									i.{nameof(BacklogItem.ModifiedBy)}.forEach(modif => {{
																			if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)}.toLowerCase() == $userId)
																				modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)} = null;
																		}});
								}}";
			var query = new IndexQuery
			{
				Query = queryString,
				QueryParameters = new Parameters
				{
					{ "userId", userId.ToLower() }
				}
			};

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}

		public void UpdateReferences(UserReference newUserReference)
		{
			if (string.IsNullOrEmpty(newUserReference.Id))
				return;
			
			var sanitisedId = GetSanitisedId(newUserReference.Id);

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.ModifiedBy)}_{sanitisedId} != null OR i.{nameof(BacklogItemIndexedForList.AssignedUserId)} == $userId
								UPDATE
								{{
									if (i.{nameof(BacklogItemIndexedForList.Assignee)}.{nameof(UserReference.Id)}.toLowerCase() == $userId) {{
										i.{nameof(BacklogItemIndexedForList.Assignee)} = $userRef;
									}}
									i.{nameof(BacklogItem.Comments)}.forEach(comment => {{
										if (comment.{nameof(Comment.Author)}.{nameof(UserReference.Id)}.toLowerCase() == $userId) {{
											comment.{nameof(Comment.Author)} = $userRef;
										}}
									}});
									i.{nameof(BacklogItem.ModifiedBy)}.forEach(modif => {{
																			if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)}.toLowerCase() == $userId)
																				modif.{nameof(BacklogItemHistoryRecord.ActionedBy)} = $userRef;
																		}});
								}}";
			var query = new IndexQuery
			{
				Query = queryString,
				QueryParameters = new Parameters
				{
					{ "userId", GetSanitisedId(newUserReference.Id).ToLower() },
					{ "userRef", newUserReference },
				}
			};

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}

		/// <summary>
		///		Replace invalid characters with empty strings. Can't pass it as a parameter, as string parameters get wrapped in '\"' when inserted
		/// </summary>
		private static string GetSanitisedId(string id) => Regex.Replace(id, @"[^\w\.@-]", "");
	}
}
