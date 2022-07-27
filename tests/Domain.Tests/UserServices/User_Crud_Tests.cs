using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using DomainResults.Common;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.UserServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class User_Crud_Tests : ConfigureTestEnvironment
{
	private readonly IUserCommandService _userCommandService;
	private readonly IUserQueryService _userQueryService;

	private const string SampleFirstName = "John";
	private const string SampleLastName = "Smith";
	private const string SampleEmail = "john.smith@gmail.com";

	public User_Crud_Tests()
	{
		_userCommandService = Container.GetService<IUserCommandService>()!;
		_userQueryService = Container.GetService<IUserQueryService>()!;
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
		var user = await _userQueryService.GetById(userRef.Id!);
		Assert.True(user.IsSuccess);
		Assert.Equal(SampleFirstName, user.Value.FirstName);
		Assert.Equal(SampleLastName, user.Value.LastName);
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
		var userUpdated = await _userCommandService.Update(userAddedRef.Id!, dto);
		await SaveChanges();

		// THEN 
		Assert.True(userUpdated.IsSuccess);
		// The ID of the edited user remains the same
		Assert.Equal(userAddedRef.Id, userUpdated.Value.Id);

		// the user's name in the DB is correct
		var user = await _userQueryService.GetById(userAddedRef.Id!);
		Assert.True(user.IsSuccess);
		Assert.Equal("David", user.Value.FirstName);
		Assert.Equal(SampleLastName, user.Value.LastName);
	}

	[Fact]
	private async Task Delete_User_Removes_It_In_Database()
	{
		// GIVEN a user
		var userAddedRef = await CreateSampleUser();

		// WHEN deleting the user
		var userDeleted = await _userCommandService.Delete(userAddedRef.Id!);
		await SaveChanges();

		// THEN 
		Assert.True(userDeleted.IsSuccess);
		// The reference of the deleted user has correct info
		Assert.Equal(userAddedRef.Id, userDeleted.Value.Id);
		Assert.Equal(userAddedRef.Name, userDeleted.Value.Name);

		// the user disappear from the DB
		var user = await _userQueryService.GetById(userAddedRef.Id!);
		Assert.Equal(DomainOperationStatus.NotFound, user.Status);
	}

	private async Task<UserReference> CreateSampleUser()
	{
		var dto = new UserAddUpdRequest
		{
			FirstName = SampleFirstName,
			LastName = SampleLastName,
			Email = SampleEmail
		};
		var userAddedRes = await _userCommandService.Create(dto);
		if (!userAddedRes.IsSuccess)
			throw new System.Exception("Can't create a user");
		await SaveChanges();

		return userAddedRes.Value;
	}
}