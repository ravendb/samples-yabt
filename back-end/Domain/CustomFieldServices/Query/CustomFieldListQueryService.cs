using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.ByCustomFieldQuery;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.CustomFieldServices.Query
{
	public class CustomFieldListQueryService : BaseService<CustomField>, ICustomFieldListQueryService
	{
		public CustomFieldListQueryService(IAsyncDocumentSession dbSession) : base(dbSession) {}

		public async Task<ListResponse<CustomFieldListGetResponse>> GetList(CustomFieldListGetRequest dto)
		{
			var query = DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>();

			var totalRecords = await query.CountAsync();

			query = ApplySorting(query, dto);
			query = query.Skip(dto.PageIndex * dto.PageSize).Take(dto.PageSize);

			var ret = await (from cf in query
					select new CustomFieldListGetResponse
					{
						Id = cf.Id,
						Name = cf.Name,
						FieldType = cf.FieldType,
						IsMandatory = cf.IsMandatory,
						BacklogItemTypes = cf.BacklogItemTypes
					}
				).ToListAsync();

			return new ListResponse<CustomFieldListGetResponse>(ret, totalRecords, dto.PageIndex, dto.PageSize);
		}

		private IRavenQueryable<CustomFieldIndexedForList> ApplySorting(IRavenQueryable<CustomFieldIndexedForList> query, CustomFieldListGetRequest dto)
		{
			if (dto.OrderBy == CustomFieldOrderColumns.Default)
			{
				dto.OrderBy = CustomFieldOrderColumns.Name;
				dto.OrderDirection = OrderDirections.Asc;
			}

			return dto.OrderBy switch
			{
				CustomFieldOrderColumns.Name				=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Name)		: query.OrderByDescending(t => t.Name),
				CustomFieldOrderColumns.Type				=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.FieldType)	: query.OrderByDescending(t => t.FieldType),
				_ => throw new NotImplementedException()
			};
		}

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
