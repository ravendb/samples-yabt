using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.UserServices;
using Raven.Yabt.Domain.UserServices.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.UserServices
{
	public class UserCommandServiceTests : ConfigureTestEnvironment
	{
		private readonly IUserCommandService _userCommandService;
		private readonly IUserQueryService _userQueryService;

		private const string SampleFirstName = "John";
		private const string SampleLastName = "Smith";
		private const string SampleEmail = "john.smith@gmail.com";

		public UserCommandServiceTests()
		{
			_userCommandService = Container.GetService<IUserCommandService>();
			_userQueryService = Container.GetService<IUserQueryService>();
		}

		[Fact]
		private async Task Added_User_Can_Be_Queried_By_Id()
		{
			// GIVEN an empty DB

			// WHEN adding a new user
			var userRef = await CreateSampleUser();

			// THEN 
			// The ID of the newly created user gets returned
			Assert.NotNull(userRef);

			// the user appears in the DB
			var user = await _userQueryService.GetById(userRef.Id);
			Assert.Equal(SampleFirstName, user.FirstName);
			Assert.Equal(SampleLastName, user.LastName);
		}

		[Fact]
		private async Task Update_User_Changes_Its_Name()
		{
			// GIVEN a user
			var userAddedRef = await CreateSampleUser();

			// WHEN changing the user's name
			var dto = new UserAddUpdRequest
			{
				FirstName = "David",
				LastName = SampleLastName,
				Email = SampleEmail
			};
			var userUpdatedRef = await _userCommandService.Update(userAddedRef.Id, dto);
			await SaveChanges();

			// THEN 
			// The ID of the edited user remains the same
			Assert.Equal(userAddedRef.Id, userUpdatedRef.Id);

			// the user's name in the DB is correct
			var user = await _userQueryService.GetById(userAddedRef.Id);
			Assert.Equal("David", user.FirstName);
			Assert.Equal(SampleLastName, user.LastName);
		}

		[Fact]
		private async Task Delete_User_Removes_It_In_Database()
		{
			// GIVEN a user
			var userAddedRef = await CreateSampleUser();

			// WHEN deleting the user
			var userDeletedRef = await _userCommandService.Delete(userAddedRef.Id);
			await SaveChanges();

			// THEN 
			// The reference of the deleted user has correct info
			Assert.Equal(userAddedRef.Id, userDeletedRef.Id);
			Assert.Equal(userAddedRef.Name, userDeletedRef.Name);

			// the user disappear from the DB
			var user = await _userQueryService.GetById(userAddedRef.Id);
			Assert.Null(user);
		}

		private async Task<UserReference> CreateSampleUser()
		{
			var dto = new UserAddUpdRequest
			{
				FirstName = SampleFirstName,
				LastName = SampleLastName,
				Email = SampleEmail
			};
			var userAddedRef = await _userCommandService.Create(dto);
			await SaveChanges();

			return userAddedRef as UserReference;
		}
	}
}
