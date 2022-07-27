using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.CustomFields;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CustomField_Query_Tests : ConfigureTestEnvironment
{
	private readonly ICustomFieldCommandService _commandService;
	private readonly ICustomFieldListQueryService _queryService;

	public CustomField_Query_Tests()
	{
		_commandService = Container.GetService<ICustomFieldCommandService>()!;
		_queryService = Container.GetService<ICustomFieldListQueryService>()!;
	}

	[Fact]
	private async Task GetList_With_No_Params()
	{
		// GIVEN 3 custom fields
		await CreateSampleCustomField();
		await CreateSampleCustomField(BacklogItemType.Bug);
		await CreateSampleCustomField(BacklogItemType.UserStory);

		// WHEN querying with no parameters
		var list = await _queryService.GetList(new CustomFieldListGetRequest());

		// THEN 
		// all 3 custom fields get returned 
		Assert.Equal(3, list.TotalRecords);
	}

	[Fact]
	private async Task GetList_By_Ids()
	{
		// GIVEN 3 custom fields
		var customFieldNoTypeId = (await CreateSampleCustomField()).Id;
		var customFieldBugId = await CreateSampleCustomField(BacklogItemType.Bug);
		await CreateSampleCustomField(BacklogItemType.UserStory);

		// WHEN querying with no parameters
		var ids = new [] { customFieldNoTypeId, customFieldBugId };
		var list = await _queryService.GetList(new CustomFieldListGetRequest { Ids = ids });

		// THEN 
		// only 2 custom fields get returned 
		Assert.Equal(2, list.TotalRecords);
		Assert.Equal(ids, list.Entries.Select(l => l.Id).OrderBy(i => i).ToArray());
	}

	[Fact]
	private async Task GetArray_By_Ids()
	{
		// GIVEN 3 custom fields
		var customFieldNoTypeId = (await CreateSampleCustomField()).Id;
		var customFieldBugId = await CreateSampleCustomField(BacklogItemType.Bug);
		await CreateSampleCustomField(BacklogItemType.UserStory);

		// WHEN querying with no parameters
		var ids = new [] { customFieldNoTypeId, customFieldBugId };
		var list = await _queryService.GetArray(new CustomFieldListGetRequest { Ids = ids });

		// THEN 
		// only 2 custom fields get returned 
		Assert.Equal(2, list.Length);
		Assert.Equal(ids, list.Select(l => l.Id).OrderBy(i => i).ToArray());
	}

	[Fact]
	private async Task GetList_For_Certain_Backlog_Item_Type_Also_Returns_Fields_With_Unspecified_Type()
	{
		// GIVEN 3 custom fields:
		//  - used for all type of backlog items
		var customFieldNoTypeId = (await CreateSampleCustomField()).Id;
		//  - used only for the b_u_g type of backlog items
		var customFieldBugId = await CreateSampleCustomField(BacklogItemType.Bug);
		//  - used only for the 'user story' type of backlog items
		await CreateSampleCustomField(BacklogItemType.UserStory);

		// WHEN querying by 'b_u_g' type
		var list = await _queryService.GetList(new CustomFieldListGetRequest { BacklogItemType = BacklogItemType.Bug });

		// THEN 
		// only 2 custom fields get returned 
		Assert.Equal(2, list.TotalRecords);
		Assert.Equal(new []{ customFieldNoTypeId, customFieldBugId }, list.Entries.Select(l => l.Id).OrderBy(i => i).ToArray());
	}

	private async Task<string> CreateSampleCustomField(BacklogItemType type)
	{
		return (await CreateSampleCustomField(
			f =>
			{
				f.BacklogItemTypes = new BacklogItemType?[] { type };
				f.Name = "Test Custom Field +"+type;
			})).Id;
	}
	private async Task<CustomFieldReferenceDto> CreateSampleCustomField(Action<CustomFieldAddRequest>? setParamsAction = null)
	{
		var dto = new CustomFieldAddRequest
		{
			Name = "Test Custom Field",
			FieldType = Database.Common.CustomFieldType.Text
		};
		setParamsAction?.Invoke(dto);
		var fieldRef = (await _commandService.Create(dto)).Value;
		await SaveChanges();

		return fieldRef;
	}
}