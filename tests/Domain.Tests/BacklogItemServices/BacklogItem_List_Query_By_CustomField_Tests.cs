using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_List_Query_By_CustomField_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemListQueryService _queryService;
	private readonly ICustomFieldCommandService _customFieldCommandService;
	private readonly IUserCommandService _userCmdService;

	private string _currentUserId = null!;

	public BacklogItem_List_Query_By_CustomField_Tests()
	{
		_commandService = Container.GetService<IBacklogItemCommandService>()!;
		_queryService = Container.GetService<IBacklogItemListQueryService>()!;
		_customFieldCommandService = Container.GetService<ICustomFieldCommandService>()!;
		_userCmdService = Container.GetService<IUserCommandService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		var currentUserResolver = Substitute.For<ICurrentUserResolver>();
		currentUserResolver.GetCurrentUserId().Returns(_ => _currentUserId);
		services.AddScoped(_ => currentUserResolver);
	}

	[Fact]
	private async Task Querying_By_Exact_Match_Of_Text_CustomField_Works()
	{
		// GIVEN 2 custom fields
		_currentUserId = await SeedCurrentUsers();
		var customFieldId = await CreateTextCustomField();
		//	and 2 backlog items with different custom field values
		var backlogItem1Id = await CreateBacklogItem(customFieldId, "val1");
		await CreateBacklogItem(customFieldId, "val2");

		// WHEN querying items by a custom field value
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				CustomField = new Dictionary<string, string> { { customFieldId, "val1" } }
			});

		// THEN 
		// the returned only one correct record 
		Assert.Single(items.Entries);
		Assert.Equal(backlogItem1Id, items.Entries[0].Id);
	}
		
	[Theory]
	[InlineData(new [] { 1, 5 },	"eq|1", 0)]
	[InlineData(new [] { 1, 5 },	"lt|3", 0)]
	[InlineData(new [] { 1, 2, 5 },	"gt|3", 2)]
	[InlineData(new [] { 2, 5 },	"lte|2", 0)]
	[InlineData(new [] { 2, 5 },	"gte|5", 1)]
	private async Task Querying_By_MoreLess_Condition_For_Numeric_CustomField_Works(int[] customValues, string filter, int indexOfValidTicket)
	{
		// GIVEN 2 custom fields
		_currentUserId = await SeedCurrentUsers();
		var customFieldId = await CreateCustomField(Database.Common.CustomFieldType.Numeric);
		//	and several backlog items with different custom field values
		var backlogItems = new List<string>();
		foreach (var cv in customValues)
			backlogItems.Add(await CreateBacklogItem(customFieldId, cv));

		// WHEN querying items by a custom field value
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				CustomField = new Dictionary<string, string> { { customFieldId, filter } }
			});

		// THEN 
		// the returned only one correct record 
		Assert.Single(items.Entries);
		Assert.Equal(backlogItems[indexOfValidTicket], items.Entries[0].Id);
	}

	[Theory]
	[InlineData(new [] { "2000-01-01", "2000-01-02" }, "eq|2000-01-02",  1)]
	[InlineData(new [] { "2000-01-01", "2000-01-02" }, "lt|2000-01-02",  0)]
	[InlineData(new [] { "2000-01-01", "2000-01-02" }, "gt|2000-01-01",  1)]
	[InlineData(new [] { "2000-01-01", "2000-01-02" }, "gte|2000-01-02", 1)]
	private async Task Querying_By_MoreLess_Condition_For_Date_CustomField_Works(string[] customValues, string filter, int indexOfValidTicket)
	{
		// GIVEN 2 custom fields
		_currentUserId = await SeedCurrentUsers();
		var customFieldId = await CreateCustomField(Database.Common.CustomFieldType.Date);
		//	and several backlog items with different custom field values
		var backlogItems = new List<string>();
		foreach (var cv in customValues)
			backlogItems.Add(await CreateBacklogItem(customFieldId, DateTime.Parse(cv)));

		// WHEN querying items by a custom field value
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				CustomField = new Dictionary<string, string> { { customFieldId, filter } }
			});

		// THEN 
		// the returned only one correct record 
		Assert.Single(items.Entries);
		Assert.Equal(backlogItems[indexOfValidTicket], items.Entries[0].Id);
	}

	[Fact]
	private async Task Querying_By_Partial_Match_Of_Text_CustomField_Works()
	{
		// GIVEN 2 custom fields
		_currentUserId = await SeedCurrentUsers();
		var customFieldId = await CreateTextCustomField();
		//	and 3 backlog items with different custom field values
		var backlogItem1Id =	await CreateBacklogItem(customFieldId, "val");
		var backlogItem2Id =	await CreateBacklogItem(customFieldId, "val1");
		await CreateBacklogItem(customFieldId, "bla");

		// WHEN querying items by a the start value of the 2 our of 3 custom fields
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				CustomField = new Dictionary<string, string> { { customFieldId, "val" } }
			});

		// THEN 
		// the returned only 2 correct record 
		Assert.Equal(2, items.TotalRecords);
		Assert.Contains(items.Entries, i => new[] { backlogItem1Id, backlogItem2Id }.Contains(i.Id));
		// and the first record is the exact match
		Assert.Equal(backlogItem1Id, items.Entries[0].Id);
	}

	[Theory]
	[InlineData("A quick FOX", "fox")]
	[InlineData("Foxy", "fox")]
	private async Task Querying_By_Token_Of_Text_CustomField_Works(string customFieldValue, string searchableCustomValue)
	{
		// GIVEN a custom field used in a backlog item
		_currentUserId = await SeedCurrentUsers();
		var customFieldId = await CreateTextCustomField();
		await CreateBacklogItem(customFieldId, customFieldValue);

		// WHEN querying items by a tokenised value
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				CustomField = new Dictionary<string, string> { { customFieldId, searchableCustomValue } }
			});

		// THEN 
		// the record found
		Assert.Single(items.Entries);
	}

	private async Task<string> SeedCurrentUsers()
	{
		var dto = new UserAddUpdRequest { FirstName = "Homer", LastName = "Simpson" };
		var homer = await _userCmdService.Create(dto);
		if (!homer.IsSuccess)
			throw new Exception("Failed to create a user for Homer");
		await SaveChanges();

		return homer.Value.Id!;
	}

	private async Task<string> CreateBacklogItem<T>(string customFieldId, T customFieldValue) where T : notnull
	{
		var dto = new BugAddUpdRequest 
		{ 
			Title = "Test_" + GetRandomString(), 
			ChangedCustomFields = new List<BacklogCustomFieldAction> { new() { CustomFieldId = customFieldId, ObjValue = customFieldValue, ActionType = ListActionType.Add } }
		};
		var addedRef = await _commandService.Create(dto);
		if (!addedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");

		await SaveChanges();

		return addedRef.Value.Id!;
	}

	private Task<string> CreateTextCustomField() => CreateCustomField(Database.Common.CustomFieldType.Text);

	private async Task<string> CreateCustomField(Database.Common.CustomFieldType type)
	{
		var dto = new CustomFieldAddRequest
		{
			Name = "Test Custom Field 1",
			FieldType = type
		};
		var customField = (await _customFieldCommandService.Create(dto)).Value;
		await SaveChanges();

		return customField.Id;
	}
}