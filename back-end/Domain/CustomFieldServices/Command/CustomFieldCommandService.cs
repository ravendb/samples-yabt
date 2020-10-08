using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command
{
	public class CustomFieldCommandService : BaseService<CustomField>, ICustomFieldCommandService
	{
		public CustomFieldCommandService(IAsyncDocumentSession dbSession) : base(dbSession)	{}

		public async Task<IDomainResult<CustomFieldReferenceDto>> Create(CustomFieldAddRequest dto)
		{
			if (await DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
							   .Where(cf => cf.Name == dto.Name)
							   .AnyAsync())
				return DomainResult.Error<CustomFieldReferenceDto>($"Custom Field with name '{dto.Name}' already exist");

			var entity = new CustomField
				{
					Name = dto.Name,
					FieldType = dto.Type
				};
			await DbSession.StoreAsync(entity);

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

		public async Task<IDomainResult<CustomFieldReferenceDto>> Rename(string id, CustomFieldRenameRequest dto)
		{
			if (dto?.Name == null)
				return DomainResult.Error<CustomFieldReferenceDto>($"New name can't be empty");

			var entity = await DbSession.LoadAsync<CustomField>(GetFullId(id));
			if (entity == null)
				return DomainResult.NotFound<CustomFieldReferenceDto>();

			entity.Name = dto.Name;

			return DomainResult.Success(GetReference(entity));
		}

		private CustomFieldReferenceDto GetReference (CustomField entity) => new CustomFieldReferenceDto { Id = entity.Id, Name = entity.Name };
	}
}
