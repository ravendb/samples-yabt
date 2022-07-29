using Raven.Client.Documents.Linq;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.Command;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands;

internal class UpdateMentionedUsersInCommentsCommand : BaseDbService, IUpdateUserReferencesCommand
{
	public UpdateMentionedUsersInCommentsCommand(IAsyncTenantedDocumentSession session): base(session) {}

	/// <inheritdoc/>
	public void ClearUserId(string userId)
		=> UpdateMentionedUsersInComments(userId, null);

	/// <inheritdoc/>
	public void UpdateReferences(UserReference newUserReference)
		=> UpdateMentionedUsersInComments(newUserReference.Id, newUserReference);

	private void UpdateMentionedUsersInComments(string? userId, UserReference? userReference)
	{
		if (string.IsNullOrEmpty(userId))
			return;
			
		var sanitisedId = userId.GetSanitisedIdForPatchQuery();
		
		// Form a patch query
		
// ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS8602, CS8073
		var idxQuery = DbSession.GetIndexQuery(
				DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
				         .Where(i => i.MentionedUser[sanitisedId] != null)
			);
#pragma warning restore CS8073, CS8602

		idxQuery.Query += $@" UPDATE
						{{
							this.{nameof(BacklogItem.Comments)}.forEach(comment => 
								{{
									let mentionedUsers = comment.{nameof(Comment.MentionedUserIds)};
									if (mentionedUsers != null)
										Object.keys(mentionedUsers).forEach(key =>
										{{
											if (mentionedUsers[key].toUpperCase() == $userId)
											{{
												// Delete old reference
												delete mentionedUsers[key];
												// Update reference (if required)
												if (!!$newMention) {{
													mentionedUsers[$newMention] = $userId;
													// Replace references in the comment's text
													let regEx = new RegExp('@'+key,'gi');
													comment.{nameof(Comment.Message)} = comment.{nameof(Comment.Message)}.replace(regEx, '@'+$newMention);
												}}
											}}
										}});
									return comment;
								}});
						}}";
		// Append parameters rather than overwriting, as it already has some from the strongly-typed WHERE condition 
		idxQuery.QueryParameters.Add("userId", sanitisedId);
		idxQuery.QueryParameters.Add("newMention", userReference?.MentionedName);

		// Add the patch to a collection
		DbSession.AddDeferredPatchQuery(idxQuery);
	}
}