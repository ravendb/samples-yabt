
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices
{
	public class BacklogItemListQueryServiceTests : ConfigureTestEnvironment
	{
		private IBacklogItemCommandService _commandService;
		private IBacklogItemListQueryService _queryService;

		private UserReference _currentUser = new UserReference { Id = "1", Name = "H. Simpson", FullName = "Homer Simpson" };

		public BacklogItemListQueryServiceTests() : base() 
		{
			_commandService = Container.GetService<IBacklogItemCommandService>();
			_queryService = Container.GetService<IBacklogItemListQueryService>();
		}

		protected override void ConfigureIocContainer(IServiceCollection services)
		{
			base.ConfigureIocContainer(services);

			var currentUserResolver = Substitute.For<ICurrentUserResolver>();
				currentUserResolver.GetCurrentUserId().Returns(_currentUser.Id);
			services.AddScoped(x => currentUserResolver);

			var userResolver = Substitute.For<IUserReferenceResolver>();
				userResolver.GetCurrentUserReference().Returns(_currentUser);
				userResolver.GetReferenceById(null).ReturnsForAnyArgs(_currentUser);
			services.AddScoped(x => userResolver);
		}

		[Theory]
		[InlineData(BacklogItemType.Bug, 1)]
		[InlineData(BacklogItemType.UserStory, 1)]
		[InlineData(BacklogItemType.Unknown, 2)]
		private async Task Querying_By_Type_Works(BacklogItemType type, int expectedRecordCount)
		{
			// GIVEN two backlog items: a bug and a user story
			var bugRef = await CreateBacklogItem<BugAddUpdRequest>();
			var usRef = await CreateBacklogItem<UserStoryAddUpdRequest>();

			// WHEN querying by type
			var items = await _queryService.GetList(new BacklogItemListGetRequest { Type = type } );

			// THEN 
			// the returned number of records is correct
			Assert.Equal(expectedRecordCount, items.Count);
			if (expectedRecordCount == 1)
			{
				// with correct type
				Assert.Equal(type, items[0].Type);
				// and with correct ID
				Assert.Equal((type == BacklogItemType.Bug) ? bugRef.Id : usRef.Id, items[0].Id);
			}
		}

		[Fact]
		private async Task Querying_By_Assigned_To_User_Works()
		{
			// GIVEN two backlog items, where only one is assigned to the user
			await CreateBacklogItem<UserStoryAddUpdRequest>();
			var assignedRef = await CreateBacklogItem<BugAddUpdRequest>();
			await _commandService.AssignToUser(assignedRef.Id, _currentUser.Id);
			await SaveChanges();

			// WHEN querying by assigned to the user
			var items = await _queryService.GetList(new BacklogItemListGetRequest { AssignedUserId = _currentUser.Id });

			// THEN 
			// the returned only 1 record
			Assert.Single(items);
			// with correct backlog ID
			Assert.Equal(assignedRef.Id, items[0].Id);
			// and with correct user ID
			Assert.Equal(_currentUser.Id, items[0].Assignee.Id);
		}

		private async Task<BacklogItemReference> CreateBacklogItem<T>() where T: BacklogItemAddUpdRequest, new ()
		{
			var dto = new T { Title = "Test_"+ GetRandomString() };
			var addedRef = await _commandService.Create(dto);
			await SaveChanges();

			return addedRef;
		}
	}
}
