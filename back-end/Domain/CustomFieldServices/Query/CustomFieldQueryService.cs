using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.CustomFieldServices.Query
{
	public class CustomFieldQueryService : BaseService<CustomField>, ICustomFieldQueryService
	{
		public CustomFieldQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		public Task<CustomFieldListGetResponse[]> GetArray(CustomFieldListGetRequest dto)
		{
			var query = DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
								 .OrderBy(cf => cf.Name);

			if (dto.Ids?.Any() == true)
			{
				IEnumerable<string> fullIds = dto.Ids.Select(GetFullId);

				query = query.Where(cf => cf.Id.In(fullIds));
			}

			return query.ProjectInto<CustomFieldListGetResponse>().ToArrayAsync();
		}

		public async Task<IDictionary<string, string>> GetFullIdsOfExistingItems(IEnumerable<string> ids)
		{
			var fullIds = ids.Select(GetFullId);

			var resolvedIds= await (from b in DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
									where b.Id.In(fullIds)
									select b.Id
									).ToArrayAsync();
			return resolvedIds.ToDictionary(id => id.GetShortId()!, id => id);
		}
	}
}
