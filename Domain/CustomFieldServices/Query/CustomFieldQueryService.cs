using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.CustomField;
using Raven.Yabt.Database.Models.CustomField.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

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
	}
}
