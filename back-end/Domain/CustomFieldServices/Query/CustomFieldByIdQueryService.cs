using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Domain.BacklogItemServices.ByCustomFieldQuery;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Query;

public class CustomFieldByIdQueryService : BaseService<CustomField>, ICustomFieldByIdQueryService
{
	private readonly IBacklogItemByCustomFieldQueryService _backlogService;

	public CustomFieldByIdQueryService(IAsyncTenantedDocumentSession dbSession, IBacklogItemByCustomFieldQueryService backlogService) : base(dbSession)
	{
		_backlogService = backlogService;
	}

	public async Task<IDomainResult<CustomFieldItemResponse>> GetById(string id)
	{
		var fullId = GetFullId(id);

		var entity = await DbSession.LoadAsync<CustomField>(fullId);
		if (entity == null)
			return DomainResult.NotFound<CustomFieldItemResponse>();

		var item = new CustomFieldItemResponse
		{
			Name					= entity.Name,
			FieldType				= entity.FieldType,
			IsMandatory				= entity.IsMandatory,
			BacklogItemTypes		= entity.BacklogItemTypes,
			UsedInBacklogItemsCount = await _backlogService.GetCountOfBacklogItemsUsingCustomField(id)
		};
		return DomainResult.Success(item);
	}
}