using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;

public class BacklogItemByIdQueryService : BaseService<BacklogItem>, IBacklogItemByIdQueryService
{
	private readonly ICustomFieldListQueryService _customFieldsService;

	public BacklogItemByIdQueryService(IAsyncTenantedDocumentSession dbSession, ICustomFieldListQueryService customFieldsService) : base(dbSession)
	{
		_customFieldsService = customFieldsService;
	}

	/// <inheritdoc/>
	public async Task<IDomainResult<BacklogItemGetResponseBase>> GetById(string id)
	{
		var fullId = GetFullId(id);

		var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
		if (ticket == null)
			return DomainResult.NotFound<BacklogItemGetResponseBase>();

		List<BacklogItemCustomFieldValue>? customFieldValues = null;
		if (ticket.CustomFields?.Any() == true)
			customFieldValues = (await _customFieldsService.GetArray(new CustomFieldListGetRequest { Ids = ticket.CustomFields.Keys })).ConvertFieldToDto(ticket.CustomFields);
			
		var dto = ticket.Type switch
		{
			BacklogItemType.Bug			=> (ticket as BacklogItemBug)		?.ConvertToDto<BacklogItemBug, 		BugGetResponse>			(customFieldValues)	as BacklogItemGetResponseBase,
			BacklogItemType.UserStory	=> (ticket as BacklogItemUserStory)	?.ConvertToDto<BacklogItemUserStory,UserStoryGetResponse>	(customFieldValues)	as BacklogItemGetResponseBase,
			BacklogItemType.Task		=> (ticket as BacklogItemTask)		?.ConvertToDto<BacklogItemTask,		TaskGetResponse>		(customFieldValues)	as BacklogItemGetResponseBase,
			BacklogItemType.Feature		=> (ticket as BacklogItemFeature)	?.ConvertToDto<BacklogItemFeature,	FeatureGetResponse>		(customFieldValues)	as BacklogItemGetResponseBase,
			_ => throw new ArgumentException($"Not supported Backlog Item Type: {ticket.Type}"),
		};
		if (dto == null)
			throw new NotSupportedException($"Failed to return Backlog Item type of {ticket.Type}");

		return DomainResult.Success(dto);
	}
}