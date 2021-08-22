using System.Text.RegularExpressions;

using Raven.Client;
using Raven.Client.Documents.Queries;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	internal class RemoveCustomFieldReferencesCommand : BaseDbService, IRemoveCustomFieldReferencesCommand
	{
		public RemoveCustomFieldReferencesCommand(IAsyncTenantedDocumentSession session): base(session) {}

		public void ClearCustomFieldId(string customFieldId)
		{
			if (string.IsNullOrEmpty(customFieldId))
				return;

			var sanitisedId = GetSanitisedId(customFieldId).ToUpper();

			// Form a patch query
			var queryString= $@"FROM INDEX '{new BacklogItems_ForList().IndexName}' AS i
								WHERE i.{nameof(BacklogItem.CustomFields)}_{sanitisedId} != null
								UPDATE
								{{
									delete i.{nameof(BacklogItem.CustomFields)}[$id];
								}}";
			var query = new IndexQuery
			{
				Query = queryString,
				QueryParameters = new Parameters
				{
					{ "id", sanitisedId }
				}
			};

			// Add the patch to a collection
			DbSession.AddDeferredPatchQuery(query);
		}

		/// <summary>
		///		Replace invalid characters with empty strings. Can't pass it as a parameter, as string parameters get wrapped in '\"' when inserted
		/// </summary>
		private static string GetSanitisedId(string id) => Regex.Replace(id, @"[^\w\.@-]", "");
	}
}
