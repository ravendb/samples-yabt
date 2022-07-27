using System.Collections.Generic;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command;

public class CustomFieldCommandService : BaseService<CustomField>, ICustomFieldCommandService
{
	private readonly IEnumerable<IRemoveCustomFieldReferencesCommand> _clearFieldReferences;

	public CustomFieldCommandService(IAsyncTenantedDocumentSession dbSession, IEnumerable<IRemoveCustomFieldReferencesCommand> clearFieldReferences) : base(dbSession)
	{
		_clearFieldReferences = clearFieldReferences;
	}

	public async Task<IDomainResult<CustomFieldReferenceDto>> Create(CustomFieldAddRequest dto)
	{
		var verificationResult = await VerifyName(null, dto.Name);
		if (!verificationResult.IsSuccess)
			return verificationResult.To<CustomFieldReferenceDto>();

		var entity = new CustomField
			{
				Name			= dto.Name,
				FieldType		= dto.FieldType,
				BacklogItemTypes= dto.BacklogItemTypes,
				IsMandatory		= dto.IsMandatory.HasValue && dto.IsMandatory.Value
			};
		await DbSession.StoreAsync(entity);

		return DomainResult.Success(GetReference(entity));
	}

	public async Task<IDomainResult<CustomFieldReferenceDto>> Update(string id, CustomFieldUpdateRequest dto)
	{
		var verificationResult = await VerifyName(id, dto.Name);
		if (!verificationResult.IsSuccess)
			return verificationResult.To<CustomFieldReferenceDto>();

		var entity = await DbSession.LoadAsync<CustomField>(GetFullId(id));
		if (entity == null)
			return DomainResult.NotFound<CustomFieldReferenceDto>();

		entity.Name				= dto.Name;
		entity.IsMandatory		= dto.IsMandatory.HasValue && dto.IsMandatory.Value;
		entity.BacklogItemTypes	= dto.BacklogItemTypes;

		return DomainResult.Success(GetReference(entity));
	}

	public async Task<IDomainResult<CustomFieldReferenceDto>> Delete(string id)
	{
		var cf = await DbSession.LoadAsync<CustomField>(GetFullId(id));
		if (cf == null)
			return DomainResult.NotFound<CustomFieldReferenceDto>();

		DbSession.Delete(cf);

		// Delete the ID in all references
		foreach (var clearFieldRef in _clearFieldReferences)
			clearFieldRef.ClearCustomFieldId(id);
			
		return DomainResult.Success(GetReference(cf));
	}

	private async Task<IDomainResult> VerifyName(string? id, string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			return DomainResult.Failed($"Name is a mandatory field");
		var query =  DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
		                      .Where(cf => cf.Name == name);
		if (!string.IsNullOrEmpty(id))
			query = query.Where(cf => cf.Id != GetFullId(id));
			
		if (await query.AnyAsync()) 
			return DomainResult.Failed($"Custom Field with name '{name}' already exist");

		return DomainResult.Success();
	}

	private CustomFieldReferenceDto GetReference (CustomField entity) => new CustomFieldReferenceDto { Id = entity.Id, Name = entity.Name };
}