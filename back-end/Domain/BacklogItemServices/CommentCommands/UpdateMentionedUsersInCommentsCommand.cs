using System.Linq;

using Raven.Client;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands
{
	internal class UpdateMentionedUsersInCommentsCommand : IUpdateUserReferencesCommand
	{
		private readonly IPatchOperationsExecuteAsync _patchOperations;
		private readonly IAsyncDocumentSession _dbSession;

		public UpdateMentionedUsersInCommentsCommand(IAsyncDocumentSession dbSession, IPatchOperationsExecuteAsync patchOperations)
		{
			_dbSession = dbSession;
			_patchOperations = patchOperations;
		}

		public void ClearUserId(string userId)
		{
			userId = GetSanitizedUserId(userId);

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.MentionedUser)}_{userId} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.Comments)}.forEach(comment => 
										{{
											let mentionedUsers = comment.{nameof(Comment.MentionedUserIds)};
											Object.keys(mentionedUsers).forEach(key =>
											{{
												if (mentionedUsers[key].toLowerCase() == '{userId}'.toLowerCase())
													delete mentionedUsers[key];
											}});
											return comment;
										}});
								}}";
			var query = new IndexQuery { Query = queryString };

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}

		public void UpdateReferences(UserReference newUserReference)
		{
			if (string.IsNullOrEmpty(newUserReference?.Id))
				return;

			var userId = GetSanitizedUserId(newUserReference.Id);

			// Form a patch query
			var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.MentionedUser)}_{userId} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.Comments)}.forEach(comment => 
										{{
											let mentionedUsers = comment.{nameof(Comment.MentionedUserIds)};
											Object.keys(mentionedUsers).forEach(key =>
											{{
												if (mentionedUsers[key].toLowerCase() == $userId.toLowerCase())
												{{
													// Replace the element in the dictionary with the new reference  
													delete mentionedUsers[key];
													mentionedUsers[$newMention] = $userId;
													// Replace references in the comment's text
													let regEx = new RegExp('@'+key,'gi');
													comment.{nameof(Comment.Message)} = comment.{nameof(Comment.Message)}.replace(regEx, '@'+$newMention);
												}}
											}});
											return comment;
										}});
								}}";
			var query = new IndexQuery
			{
				Query = queryString,
				QueryParameters = new Parameters
				{
					{ "userId", userId },
					{ "newMention", newUserReference.MentionedName },
				}
			};

			// Add the patch to a collection
			_patchOperations.AddDeferredPatchQuery(query);
		}
		
		private static string GetSanitizedUserId(string userId) => userId.Split(new[]{'/'}).Last();
	}
}
