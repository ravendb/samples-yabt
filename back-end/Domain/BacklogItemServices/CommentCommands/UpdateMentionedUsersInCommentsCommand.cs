using System.Text.RegularExpressions;

using Raven.Client;
using Raven.Client.Documents.Queries;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.UserServices.Command;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands;

internal class UpdateMentionedUsersInCommentsCommand : BaseDbService, IUpdateUserReferencesCommand
{
	public UpdateMentionedUsersInCommentsCommand(IAsyncTenantedDocumentSession session): base(session) {}

	public void ClearUserId(string userId)
	{
		var sanitisedId = GetSanitizedUserId(userId);

		// Form a patch query
		var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.MentionedUser)}_{sanitisedId} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.Comments)}.forEach(comment => 
										{{
											let mentionedUsers = comment.{nameof(Comment.MentionedUserIds)};
											if (mentionedUsers != null)
												Object.keys(mentionedUsers).forEach(key =>
												{{
													if (mentionedUsers[key].toUpperCase() == $userId.toUpperCase())
														delete mentionedUsers[key];
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
			}
		};

		// Add the patch to a collection
		DbSession.AddDeferredPatchQuery(query);
	}
		
	public void UpdateReferences(UserReference newUserReference)
	{
		if (string.IsNullOrEmpty(newUserReference.Id))
			return;

		var sanitisedId = GetSanitizedUserId(newUserReference.Id);

		// Form a patch query
		var queryString = $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItemIndexedForList.MentionedUser)}_{sanitisedId} != null
								UPDATE
								{{
									i.{nameof(BacklogItem.Comments)}.forEach(comment => 
										{{
											let mentionedUsers = comment.{nameof(Comment.MentionedUserIds)};
											if (mentionedUsers != null)
												Object.keys(mentionedUsers).forEach(key =>
												{{
													if (mentionedUsers[key].toUpperCase() == $userId.toUpperCase())
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
				{ "userId", newUserReference.Id },
				{ "newMention", newUserReference.MentionedName },
			}
		};

		// Add the patch to a collection
		DbSession.AddDeferredPatchQuery(query);
	}
		
	/// <summary>
	///		Replace invalid characters with empty strings. Can't pass it as a parameter, as string parameters get wrapped in '\"' when inserted
	/// </summary>
	private static string GetSanitizedUserId(string userId) =>  Regex.Replace(userId, @"[^\w\.@-]", "");
}