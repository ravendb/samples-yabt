using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Query;

public class CustomFieldListQueryService : BaseService<CustomField>, ICustomFieldListQueryService
{
	public CustomFieldListQueryService(IAsyncTenantedDocumentSession dbSession) : base(dbSession) {}

	public async Task<ListResponse<CustomFieldListGetResponse>> GetList(CustomFieldListGetRequest dto)
	{
		var query = DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>();
		query = ApplyFilters(query, dto);
			
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

	public Task<CustomFieldListGetResponse[]> GetArray(CustomFieldListGetRequest dto)
	{
		var query = DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>();
		query = ApplyFilters(query, dto);

		return query.OrderBy(cf => cf.Name)
		            .ProjectInto<CustomFieldListGetResponse>()
		            .ToArrayAsync();
	}

	private IRavenQueryable<CustomFieldIndexedForList> ApplyFilters(IRavenQueryable<CustomFieldIndexedForList> query, CustomFieldListGetRequest dto)
	{
		if (dto.Ids?.Any() == true)
		{
			IEnumerable<string> fullIds = dto.Ids.Select(GetFullId);
			query = query.Where(cf => cf.Id.In(fullIds));
		}
		if (dto.BacklogItemType.HasValue)
			query = query.Where(cf => cf.BacklogItemTypes!.Any() == false || cf.BacklogItemTypes!.Contains(dto.BacklogItemType));
			
		return query;
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
			CustomFieldOrderColumns.Name 
				or CustomFieldOrderColumns.Default	=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Name)		: query.OrderByDescending(t => t.Name),
			CustomFieldOrderColumns.Type			=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.FieldType)	: query.OrderByDescending(t => t.FieldType),
			_ => throw new ArgumentOutOfRangeException($"Unsupported 'order by' - {dto.OrderBy}")
		};
	}

	public async Task<IList<string>> VerifyExistingItems(IEnumerable<string> ids)
	{
		var fullIds = ids.Select(GetFullId);

		var resolvedIds= await (from b in DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
				where b.Id.In(fullIds)
				select b.Id
			).ToArrayAsync();
		return resolvedIds.Select(id => id.GetShortId()!).ToList();
	}
}