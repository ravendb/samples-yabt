using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.CustomFields
{
	public class CustomFieldCrudTests : ConfigureTestEnvironment
	{
		private readonly ICustomFieldCommandService _commandService;
		private readonly ICustomFieldQueryService _queryService;

		public CustomFieldCrudTests() : base()
		{
			_commandService = Container.GetService<ICustomFieldCommandService>();
			_queryService = Container.GetService<ICustomFieldQueryService>();
		}

		[Fact]
		private async Task Added_CustomField_Can_Be_Queried()
		{
			// GIVEN an empty DB

			// WHEN adding a new custom field
			var customFieldRef = await CreateSampleCustomField();

			// THEN 
			// The returned ID of the newly created entity gets returned
			Assert.NotNull(customFieldRef);

			// the entity appears in the DB
			var record = await _queryService.GetArray(new CustomFieldListGetRequest { Ids = new[] { customFieldRef.Id } });
			Assert.Single(record);
			Assert.Equal(customFieldRef.Name, record.Single().Name);
		}

		[Fact]
		private async Task Renamed_CustomField_Persistes_The_New_Name()
		{
			// GIVEN a custom field
			var customFieldRef = await CreateSampleCustomField();

			// WHEN renaming it
			await _commandService.Rename(customFieldRef.Id, new CustomFieldRenameRequest { Name = "New name" });
			await SaveChanges();

			// THEN 
			// the entity appears in the DB with the new name
			var record = await _queryService.GetArray(new CustomFieldListGetRequest { Ids = new[] { customFieldRef.Id } });
			Assert.Single(record);
			Assert.Equal("New name", record.Single().Name);
		}

		[Fact]
		private async Task Deleted_CustomField_Disappears_From_Db()
		{
			// GIVEN a custom field
			var customFieldRef = await CreateSampleCustomField();

			// WHEN deleting
			var customFieldDeletedRef = await _commandService.Delete(customFieldRef.Id);
			await SaveChanges();

			// THEN 
			// The returned ID of the deleted entity is correct
			Assert.Equal(customFieldRef.Id, customFieldDeletedRef.Id);

			// the entity disappears from the DB
			var record = await _queryService.GetArray(new CustomFieldListGetRequest { Ids = new[] { customFieldRef.Id } });
			Assert.False(record.Any());
		}

		private async Task<CustomFieldReference> CreateSampleCustomField()
		{
			var dto = new CustomFieldAddRequest
			{
				Name = "Test Custom Field",
				Type = Database.Common.CustomFieldType.Text
			};
			var fieldRef = await _commandService.Create(dto);
			await SaveChanges();

			return fieldRef;
		}
	}
}
