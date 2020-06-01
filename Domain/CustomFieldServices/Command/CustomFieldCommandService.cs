using System;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.CustomField;
using Raven.Yabt.Database.Models.CustomField.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Command
{
	public class CustomFieldCommandService : BaseService<CustomField>, ICustomFieldCommandService
	{
		public CustomFieldCommandService(IAsyncDocumentSession dbSession) : base(dbSession)	{}

		public async Task<CustomFieldReference> Create(CustomFieldAddRequest dto)
		{
			if (await DbSession.Query<CustomFieldIndexedForList, CustomFields_ForList>()
							   .Where(cf => cf.Name == dto.Name)
							   .AnyAsync())
				throw new ApplicationException($"Custom Field with name '{dto.Name}' already exist");

			var entity = new CustomField
				{
					Name = dto.Name,
					FieldType = dto.Type
				};
			await DbSession.StoreAsync(entity);

			return GetReference(entity);
		}

		public async Task<CustomFieldReference?> Delete(string id)
		{
			// TODO: Prohibit deletion if there are any references

			var cf = await DbSession.LoadAsync<CustomField>(GetFullId(id));
			if (cf == null)
				return null;

			DbSession.Delete(cf);

			return GetReference(cf);
		}

		public async Task<CustomFieldReference?> Rename(string id, CustomFieldRenameRequest dto)
		{
			if (dto?.Name == null)
				throw new ArgumentNullException(nameof(dto));

			var entity = await DbSession.LoadAsync<CustomField>(GetFullId(id));
			if (entity == null)
				return null;

			entity.Name = dto.Name;

			return GetReference(entity);
		}

		private CustomFieldReference GetReference (CustomField entity) => new CustomFieldReference { Id = entity.Id, Name = entity.Name };
	}
}
