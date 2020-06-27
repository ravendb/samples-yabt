using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices
{
	public class BacklogItemListQueryByCustomFieldTests : ConfigureTestEnvironment
	{
		private readonly IBacklogItemCommandService _commandService;
		private readonly IBacklogItemListQueryService _queryService;
		private readonly ICustomFieldCommandService _customFieldCommandService;
		private readonly IUserCommandService _userCmdService;

		private string _currentUserId;

		public BacklogItemListQueryByCustomFieldTests() : base()
		{
			_commandService = Container.GetService<IBacklogItemCommandService>();
			_queryService = Container.GetService<IBacklogItemListQueryService>();
			_customFieldCommandService = Container.GetService<ICustomFieldCommandService>();
			_userCmdService = Container.GetService<IUserCommandService>();
		}

		protected override void ConfigureIocContainer(IServiceCollection services)
		{
			base.ConfigureIocContainer(services);

			var currentUserResolver = Substitute.For<ICurrentUserResolver>();
				currentUserResolver.GetCurrentUserId().Returns(c => _currentUserId);
			services.AddScoped(x => currentUserResolver);
		}

		[Fact]
		private async Task Querying_By_Exact_Match_Of_Text_CustomField_Works()
		{
			// GIVEN 2 custom fields
			_currentUserId = await SeedCurrentUsers();
			var customFieldId = await CreateTextCustomField();
			//	and 2 backlog items with different custom field values
			var backlogItem1Id = await CreateBacklogItem(customFieldId, "val1");
			var backlogItem2Id = await CreateBacklogItem(customFieldId, "val2");

			// WHEN querying items by a custom field value
			var items = await _queryService.GetList(
						new BacklogItemListGetRequest
						{
							CustomField = new Dictionary<string, string> { { customFieldId, "val1" } }
						});

			// THEN 
			// the returned only one correct record 
			Assert.Single(items);
			Assert.Equal(backlogItem1Id, items[0].Id);
		}

		[Fact]
		private async Task Querying_By_Partial_Match_Of_Text_CustomField_Works()
		{
			// GIVEN 2 custom fields
			_currentUserId = await SeedCurrentUsers();
			var customFieldId = await CreateTextCustomField();
			//	and 3 backlog items with different custom field values
			var backlogItem1Id = await CreateBacklogItem(customFieldId, "val");
			var backlogItem2Id = await CreateBacklogItem(customFieldId, "val1");
			var backlogItem3Id = await CreateBacklogItem(customFieldId, "bla");

			// WHEN querying items by a the start value of the 2 our of 3 custom fields
			var items = await _queryService.GetList(
						new BacklogItemListGetRequest
						{
							CustomField = new Dictionary<string, string> { { customFieldId, "val" } }
						});

			// THEN 
			// the returned only 2 correct record 
			Assert.Equal(2, items.Count);
			Assert.Contains(items, i => new[] { backlogItem1Id, backlogItem2Id }.Contains(i.Id));
			// and the first record is the exact match
			Assert.Equal(backlogItem1Id, items[0].Id);
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
			Assert.Single(items);
		}

		private async Task<string> SeedCurrentUsers()
		{
			var dto = new Domain.UserServices.DTOs.UserAddUpdRequest { FirstName = "Homer", LastName = "Simpson" };
			var homer = await _userCmdService.Create(dto);
			await SaveChanges();

			return homer.Id;
		}

		private async Task<string> CreateBacklogItem(string customFieldId, string customFieldValue)
		{
			var dto = new BugAddUpdRequest 
				{ 
					Title = "Test_" + GetRandomString(), 
					CustomFields = new Dictionary<string, object> { { customFieldId, customFieldValue } }
				};
			var addedRef = await _commandService.Create(dto);
			await SaveChanges();

			return addedRef.Id;
		}

		private async Task<string> CreateTextCustomField()
		{
			var dto = new CustomFieldAddRequest
				{
					Name = "Test Custom Field 1",
					Type = Database.Common.CustomFieldType.Text
				};
			var customField = await _customFieldCommandService.Create(dto);
			await SaveChanges();

			return customField.Id;
		}
	}
}
