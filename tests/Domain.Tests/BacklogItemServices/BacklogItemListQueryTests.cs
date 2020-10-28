using System;
using System.Linq;
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
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices
{
	public class BacklogItemListQueryServiceTests : ConfigureTestEnvironment
	{
		private readonly IBacklogItemCommandService _commandService;
		private readonly IBacklogItemListQueryService _queryService;
		private readonly IUserCommandService _userCommandService;

		private readonly string _currentUserId;

		public BacklogItemListQueryServiceTests() : base() 
		{
			_commandService = Container.GetService<IBacklogItemCommandService>();
			_queryService = Container.GetService<IBacklogItemListQueryService>();
			_userCommandService = Container.GetService<IUserCommandService>();

			_currentUserId = CreateSampleUser().Result.Id!;
		}

		protected override void ConfigureIocContainer(IServiceCollection services)
		{
			base.ConfigureIocContainer(services);

			var currentUserResolver = Substitute.For<ICurrentUserResolver>();
				currentUserResolver.GetCurrentUserId().Returns(c => _currentUserId);
			services.AddScoped(x => currentUserResolver);
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
			Assert.Equal(expectedRecordCount, items.TotalRecords);
			if (expectedRecordCount == 1)
			{
				// with correct type
				Assert.Equal(type, items.Entries.First().Type);
				// and with correct ID
				Assert.Equal((type == BacklogItemType.Bug) ? bugRef.Id : usRef.Id, items.Entries.First().Id);
			}
		}

		[Fact]
		private async Task Querying_By_Assigned_To_User_Works()
		{
			// GIVEN two backlog items, where only one is assigned to the user
			await CreateBacklogItem<UserStoryAddUpdRequest>();
			var assignedRef = await CreateBacklogItem<BugAddUpdRequest>();
			await _commandService.AssignToUser(assignedRef.Id, _currentUserId);
			await SaveChanges();

			// WHEN querying by assigned to the user
			var items = await _queryService.GetList(new BacklogItemListGetRequest { AssignedUserId = _currentUserId });

			// THEN 
			// the returned only 1 record
			Assert.Equal(1, items.TotalRecords);
			// with correct backlog ID
			Assert.Equal(assignedRef.Id, items.Entries.First().Id);
			// and with correct user ID
			Assert.Equal(_currentUserId, items.Entries.First().Assignee.Id);
		}

		private async Task<BacklogItemReference> CreateBacklogItem<T>() where T: BacklogItemAddUpdRequestBase, new ()
		{
			var dto = new T { Title = "Test_"+ GetRandomString() };
			var added = await _commandService.Create(dto);
			if (!added.IsSuccess)
				throw new Exception("Failed to create a backlog item");
			await SaveChanges();

			return added.Value;
		}

		private async Task<UserReference> CreateSampleUser()
		{
			var dto = new UserAddUpdRequest
				{
					FirstName = "Homer",
					LastName = "Simpson"
				};
			var userAdded = await _userCommandService.Create(dto);
			if (!userAdded.IsSuccess)
				throw new Exception("Failed to create a user");
			await SaveChanges();

			return userAdded.Value;
		}
	}
}
