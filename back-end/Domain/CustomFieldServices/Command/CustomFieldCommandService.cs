using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.ByCustomFieldQuery;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command
{
	public class CustomFieldCommandService : BaseService<CustomField>, ICustomFieldCommandService
	{
		private readonly IBacklogItemByCustomFieldQueryService _backlogService;

		public CustomFieldCommandService(IAsyncDocumentSession dbSession, IBacklogItemByCustomFieldQueryService backlogService) : base(dbSession)
		{
			_backlogService = backlogService;
		}

		public async Task<IDomainResult<CustomFieldReferenceDto>> Create(CustomFieldAddRequest dto)
		{
			var verificationResult = await VerifyName(null, dto.Name);
			if (!verificationResult.IsSuccess)
				return verificationResult.To<CustomFieldReferenceDto>();

			var entity = new CustomField
				{
					Name = dto.Name,
					FieldType = dto.Type,
					BacklogItemTypes = dto.BacklogItemTypes,
					IsMandatory = dto.IsMandatory
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

			entity.Name = dto.Name;
			entity.IsMandatory = dto.IsMandatory;
			entity.BacklogItemTypes = dto.BacklogItemTypes;

			return DomainResult.Success(GetReference(entity));
		}

		public async Task<IDomainResult<CustomFieldReferenceDto>> Delete(string id)
		{
			// TODO: Prohibit deletion if there are any references

			var cf = await DbSession.LoadAsync<CustomField>(GetFullId(id));
			if (cf == null)
				return DomainResult.NotFound<CustomFieldReferenceDto>();

			DbSession.Delete(cf);

			return DomainResult.Success(GetReference(cf));
		}

		private async Task<IDomainResult> VerifyName(string? id, string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return DomainResult.Failed($"Name is a mandatory field");
			var query =  DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
			                      .Where(cf => cf.Name == name);
			if (!string.IsNullOrEmpty(id))
				query = query.Where(cf => cf.Id != id);
			
			if (await query.AnyAsync()) 
				return DomainResult.Failed($"Custom Field with name '{name}' already exist");

			return DomainResult.Success();
		}

		private CustomFieldReferenceDto GetReference (CustomField entity) => new CustomFieldReferenceDto { Id = entity.Id, Name = entity.Name };
	}
}
