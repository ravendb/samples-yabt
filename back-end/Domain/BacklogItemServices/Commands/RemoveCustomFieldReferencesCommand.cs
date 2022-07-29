using Raven.Client.Documents.Linq;

using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands;

internal class RemoveCustomFieldReferencesCommand : BaseDbService, IRemoveCustomFieldReferencesCommand
{
	public RemoveCustomFieldReferencesCommand(IAsyncTenantedDocumentSession session): base(session) {}

	public void ClearCustomFieldId(string customFieldId)
	{
		if (string.IsNullOrEmpty(customFieldId))
			return;

		var sanitisedId = customFieldId.GetSanitisedIdForPatchQuery();

		// Form a patch query

// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
#pragma warning disable CS8073
		var idxQuery = DbSession.GetIndexQuery(
				DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
				         .Where(i => i.CustomFields![sanitisedId] != null)
			);
#pragma warning restore CS8073

		idxQuery.Query += $@"UPDATE
							{{
								delete this.{nameof(BacklogItem.CustomFields)}[$id];
							}}";
		// Append parameters rather than overwriting, as it already has some from the strongly-typed WHERE condition 
		idxQuery.QueryParameters.Add("id", sanitisedId);


		// Add the patch to a collection
		DbSession.AddDeferredPatchQuery(idxQuery);
	}
}