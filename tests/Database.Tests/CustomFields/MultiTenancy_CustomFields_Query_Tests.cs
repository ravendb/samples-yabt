using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.CustomFields.Indexes;

using Xunit;

namespace Raven.Yabt.Database.Tests.CustomFields;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MultiTenancy_CustomFields_Query_Tests : ConfigureTestEnvironment
{
	[Fact]
	public async Task Querying_CustomField_By_Entity_Filters_Result_By_Tenant()
	{
		// GIVEN 2 custom fields with different TenantIDs
		var myField = await CreateMySampleCustomField();
		await CreateNotMySampleCustomField();
			
		// WHEN query all fields
		var fields = (await DbSession.Query<CustomField>().ToArrayAsync())
		             .Select(t => t.Id).ToArray();
			
		// THEN the list has the only my field
		Assert.Equal(new [] { myField.Id }, fields);
	}

	[Fact]
	public async Task Querying_CustomField_By_Index_Filters_Result_By_Tenant()
	{
		// GIVEN 2 custom fields with different TenantIDs
		var myField = await CreateMySampleCustomField();
		await CreateNotMySampleCustomField();
			
		// WHEN query all fields
		var fields = await DbSession.Query<CustomField,CustomFields_ForList>().ToArrayAsync();
		var fieldIds = fields.Select(t => t.Id).ToArray();
			
		// THEN the list has only my field
		Assert.Equal(new [] { myField.Id }, fieldIds);
	}
	
	[Fact]
	public async Task Querying_CustomField_By_Index_And_Selecting_Fields_Filters_Result_By_Tenant()
	{
		// GIVEN 2 custom fields with different TenantIDs
		var myField = await CreateMySampleCustomField();
		await CreateNotMySampleCustomField();
			
		// WHEN query all fields
		var query = DbSession.Query<CustomField,CustomFields_ForList>().Select(t => t.Id);
		var fieldIds = await query.ToArrayAsync();
			
		// THEN the list has only my field
		Assert.Equal(new [] { myField.Id }, fieldIds);
	}
	
	private Task<CustomField> CreateMySampleCustomField(Action<CustomField>? setExtraPropertiesAction = null) 
		=> CreateSampleCustomField(setExtraPropertiesAction);

	private async Task<CustomField> CreateNotMySampleCustomField(Action<CustomField>? setExtraPropertiesAction = null)
	{
		IsMyTenantFlag = false;
		var task = await CreateSampleCustomField(setExtraPropertiesAction);
		IsMyTenantFlag = true;
		return task;
	}
		
	private async Task<CustomField> CreateSampleCustomField(Action<CustomField>? setExtraPropertiesAction = null)
	{
		var entity = new CustomField { Name = "Test field", FieldType = CustomFieldType.Text };
		setExtraPropertiesAction?.Invoke(entity);
			
		await DbSession.StoreAsync(entity);
		await DbSession.SaveChangesAsync();

		return entity;
	}
}