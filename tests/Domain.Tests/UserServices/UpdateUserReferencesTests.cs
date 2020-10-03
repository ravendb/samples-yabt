using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices;
using Raven.Yabt.Domain.UserServices.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.UserServices
{
	public class UpdateUserReferencesTests : ConfigureTestEnvironment
	{
		private readonly IUserCommandService _userCommandService;
		private readonly IBacklogItemCommandService _backlogCommandService;
		private readonly IBacklogItemByIdQueryService _backlogQueryService;
		private ICurrentUserResolver _currentUserResolver;

		private string _currentUserId;

		public UpdateUserReferencesTests()
		{
			_userCommandService = Container.GetService<IUserCommandService>();
			_backlogCommandService = Container.GetService<IBacklogItemCommandService>();
			_backlogQueryService = Container.GetService<IBacklogItemByIdQueryService>();
		}

		protected override void ConfigureIocContainer(IServiceCollection services)
		{
			base.ConfigureIocContainer(services);

			_currentUserResolver = Substitute.For<ICurrentUserResolver>();
			_currentUserResolver.GetCurrentUserId().Returns(c => _currentUserId);
			services.AddScoped(x => _currentUserResolver);
		}

		[Fact]
		private async Task Deleting_User_Clears_UserId_In_Backlog_Items()
		{
			// GIVEN a user and a backlog item created by him
			var userRef = await CreateSampleUser();
			_currentUserId = userRef.Id;
			var backlogItemRef = await CreateBacklogItem();

			// WHEN the user gets deleted
			await _userCommandService.Delete(userRef.Id);
			await SaveChanges();

			// THEN all the references to the deleted user lack of its ID
			var item = (await _backlogQueryService.GetById(backlogItemRef.Id)).Value;
			Assert.Null(item.Created.ActionedBy.Id);
			Assert.Null(item.LastUpdated.ActionedBy.Id);
			// and the user's name remains
			Assert.Equal(userRef.Name, item.Created.ActionedBy.Name);
			Assert.Equal(userRef.Name, item.LastUpdated.ActionedBy.Name);
		}

		[Fact]
		private async Task Updating_Users_Name_Updates_References_In_Backlog_Items()
		{
			// GIVEN a user and a backlog item created by him
			var userRef = await CreateSampleUser();
			_currentUserId = userRef.Id;
			var backlogItemRef = await CreateBacklogItem();

			// WHEN the user changes its name
			var dto = new UserAddUpdRequest { FirstName = "Marge", LastName = "Simpson" };
			var updatedRef = await _userCommandService.Update(userRef.Id, dto);
			await SaveChanges();

			// THEN all the references to the updated user have the new name
			var item = (await _backlogQueryService.GetById(backlogItemRef.Id)).Value;
			Assert.Equal(updatedRef.FullName, item.Created.ActionedBy.FullName);
			Assert.Equal(updatedRef.FullName, item.LastUpdated.ActionedBy.FullName);
		}

		[Fact]
		private async Task Updating_Users_Name_Doesnt_Messup_Other_References_In_Backlog_Items()
		{
			// GIVEN 2 users and a backlog item changed by both
			var homerRef = await CreateSampleUser();
			var nedRef = await CreateSampleUser("Ned", "Flanders");
			_currentUserId = homerRef.Id;
			var backlogItemRef = await CreateBacklogItem();
			_currentUserId = nedRef.Id;
			await _backlogCommandService.AssignToUser(backlogItemRef.Id, nedRef.Id);

			// WHEN the one user changes its name
			var dto = new UserAddUpdRequest { FirstName = "Marge", LastName = "Simpson" };
			var updatedRef = await _userCommandService.Update(homerRef.Id, dto);
			await SaveChanges();

			// THEN only "Created" property has the references updated
			var item = (await _backlogQueryService.GetById(backlogItemRef.Id)).Value;
			Assert.Equal(updatedRef.FullName, item.Created.ActionedBy.FullName);
			// and "LastUpdated" property hasn't changed
			Assert.Equal(nedRef.FullName, item.LastUpdated.ActionedBy.FullName);
		}

		private async Task<UserReference> CreateSampleUser(string firstName = "Homer", string lastName = "Simpson")
		{
			var dto = new UserAddUpdRequest
				{
					FirstName = firstName,
					LastName = lastName
				};
			var userAddedRef = await _userCommandService.Create(dto);
			await SaveChanges();

			return userAddedRef as UserReference;
		}

		private async Task<BacklogItemReference> CreateBacklogItem()
		{
			var dto = new BugAddUpdRequest { Title = "Test_" + GetRandomString() };
			var addedRef = await _backlogCommandService.Create(dto);
			if (!addedRef.IsSuccess)
				throw new Exception("Failed to create a backlog item");

			await SaveChanges();

			return addedRef.Value;
		}
	}
}
