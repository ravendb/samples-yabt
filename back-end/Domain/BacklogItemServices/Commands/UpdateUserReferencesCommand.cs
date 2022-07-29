using Raven.Client.Documents.Linq;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.UserServices.Command;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands;

internal class UpdateUserReferencesCommand : BaseDbService, IUpdateUserReferencesCommand
{
	public UpdateUserReferencesCommand(IAsyncTenantedDocumentSession session): base(session) {}

	/// <inheritdoc/>
	public void ClearUserId(string userId)
		=> UpdateUserReferenceInBacklogItems(userId, null);

	/// <inheritdoc/>
	public void UpdateReferences(UserReference newUserReference)
		=> UpdateUserReferenceInBacklogItems(newUserReference.Id, newUserReference);
	
	private void UpdateUserReferenceInBacklogItems(string? userId, UserReference? userReference)
	{
		if (string.IsNullOrEmpty(userId))
			return;
			
		var sanitisedId = userId.GetSanitisedIdForPatchQuery();
		
		// Form a patch query

// ReSharper disable once ConditionIsAlwaysTrueOrFalse
#pragma warning disable CS8073
		var idxQuery = DbSession.GetIndexQuery(
					DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
                                       .Where(i 
	                                               => i.ModifiedByUser[sanitisedId] != null	// Any modification done by the user (e.g. a comment or an update)
	                                               || i.AssignedUserId == sanitisedId		// The assignee of the ticket
                                           )
						);
#pragma warning restore CS8073

		idxQuery.Query += $@" UPDATE
						{{
							if (this.{nameof(BacklogItemIndexedForList.Assignee)}.{nameof(UserReference.Id)}.toUpperCase() == $userId)
								this.{nameof(BacklogItemIndexedForList.Assignee)} = $userRef;
							
							this.{nameof(BacklogItem.Comments)}.forEach(comment => {{
								if (comment.{nameof(Comment.Author)}.{nameof(UserReference.Id)}.toUpperCase() == $userId) {{
									if ($userRef == null)
										// Remove the user's ID in the reference but keep the old name 
										comment.{nameof(Comment.Author)}.{nameof(UserReference.Id)} = null;
									else
										comment.{nameof(Comment.Author)} = $userRef;
								}}
							}});
							this.{nameof(BacklogItem.ModifiedBy)}.forEach(modif => {{
																	if (modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)}.toUpperCase() == $userId) {{
																		if ($userRef == null)
																			// Remove the user's ID in the reference but keep the old name 
																			modif.{nameof(BacklogItemHistoryRecord.ActionedBy)}.{nameof(UserReference.Id)} = null;
																		else
																			modif.{nameof(BacklogItemHistoryRecord.ActionedBy)} = $userRef;
																	}}
																}});
						}}";
		// Append parameters rather than overwriting, as it already has some from the strongly-typed WHERE condition 
		idxQuery.QueryParameters.Add("userId", sanitisedId);
		idxQuery.QueryParameters.Add("userRef", userReference);

		// Add the patch to a collection
		DbSession.AddDeferredPatchQuery(idxQuery);
	}
}