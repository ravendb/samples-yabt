using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.CustomFields;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CustomField_Crud_Tests : ConfigureTestEnvironment
{
	private readonly ICustomFieldCommandService _commandService;
	private readonly ICustomFieldByIdQueryService _queryService;

	public CustomField_Crud_Tests()
	{
		_commandService = Container.GetService<ICustomFieldCommandService>()!;
		_queryService = Container.GetService<ICustomFieldByIdQueryService>()!;
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
		var record = (await _queryService.GetById(customFieldRef.Id)).Value;
		Assert.Equal(customFieldRef.Name, record.Name);
	}

	[Fact]
	private async Task Renamed_CustomField_Persists_The_New_Name()
	{
		// GIVEN a custom field
		var customFieldRef = await CreateSampleCustomField();

		// WHEN renaming it
		var response = await _commandService.Update(customFieldRef.Id, new CustomFieldUpdateRequest { Name = "New name" });
		await SaveChanges();

		// THEN 
		// renaming was successful
		Assert.True(response.IsSuccess);
		// the entity appears in the DB with the new name
		var record = (await _queryService.GetById(customFieldRef.Id)).Value;
		Assert.Equal("New name", record.Name);
	}

	[Fact]
	private async Task Deleted_CustomField_Disappears_From_Db()
	{
		// GIVEN a custom field
		var customFieldRef = await CreateSampleCustomField();

		// WHEN deleting
		var response = await _commandService.Delete(customFieldRef.Id);
		await SaveChanges();

		// THEN 
		// Deletion was successful
		Assert.True(response.IsSuccess);
		// The returned ID of the deleted entity is correct
		Assert.Equal(customFieldRef.Id, response.Value.Id);

		// the entity disappears from the DB
		var record = await _queryService.GetById(customFieldRef.Id);
		Assert.False(record.IsSuccess);
	}

	private async Task<CustomFieldReferenceDto> CreateSampleCustomField()
	{
		var dto = new CustomFieldAddRequest
		{
			Name = "Test Custom Field",
			FieldType = Database.Common.CustomFieldType.Text
		};
		var fieldRef = (await _commandService.Create(dto)).Value;
		await SaveChanges();

		return fieldRef;
	}
}