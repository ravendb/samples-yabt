using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;
using Raven.Yabt.Domain.UserServices.Query;
using Raven.Yabt.Domain.UserServices.Query.DTOs;

using Xunit;
// ReSharper disable UnusedMethodReturnValue.Local

namespace Raven.Yabt.Domain.Tests.UserServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class UserList_Query_Tests : ConfigureTestEnvironment
{
	private readonly IUserCommandService _userCommandService;
	private readonly IUserQueryService _userQueryService;

	public UserList_Query_Tests()
	{
		_userCommandService = Container.GetService<IUserCommandService>()!;
		_userQueryService = Container.GetService<IUserQueryService>()!;
	}

	[Fact]
	private async Task Users_Can_Be_Queried()
	{
		// GIVEN 4 users
		await CreateUser("Homer", "Simpson",  "homer@gmail.com");
		await CreateUser("Marge", "Simpson",  "marge@gmail.com");
		await CreateUser("Moe",   "Szyslak",  "moe@gmail.com");
		await CreateUser("Ned",   "Flanders", "ned@gmail.com");

		// WHEN querying a list of all users
		var userList = await _userQueryService.GetList(new UserListGetRequest());

		// THEN 
		// The number of returned records is correct
		Assert.Equal(4, userList.TotalRecords);
	}

	[Theory]
	[InlineData(UsersOrderColumns.Default,			Common.OrderDirections.Asc,  "N. Flanders",	"M. Szyslak")]
	[InlineData(UsersOrderColumns.Name,				Common.OrderDirections.Asc,  "N. Flanders",	"M. Szyslak")]
	[InlineData(UsersOrderColumns.Email,			Common.OrderDirections.Desc, "N. Flanders", "H. Simpson")]
	[InlineData(UsersOrderColumns.RegistrationDate, Common.OrderDirections.Desc, "M. Simpson",  "M. Szyslak")]
	private async Task Users_Can_Be_Queried_With_Sorting_Order(UsersOrderColumns orderBy, Common.OrderDirections orderDirection, string firstNameWithInitials, string lastNameWithInitials)
	{
		// GIVEN 4 users
		await CreateUser("Moe",   "Szyslak",  "3@gmail.com");
		await CreateUser("Ned",   "Flanders", "4@gmail.com");
		await CreateUser("Homer", "Simpson",  "1@gmail.com");
		await CreateUser("Marge", "Simpson",  "2@gmail.com");

		// WHEN querying a list of all users with specified sorting order
		var userList = await _userQueryService.GetList(new UserListGetRequest { OrderBy = orderBy, OrderDirection = orderDirection });

		// THEN 
		// The returned result is in the correct sorting order
		Assert.Equal(firstNameWithInitials, userList.Entries.First().NameWithInitials);
		Assert.Equal(lastNameWithInitials,  userList.Entries.Last().NameWithInitials);
	}

	private async Task<UserReference> CreateUser(string firstName, string lastName, string email)
	{
		var dto = new UserAddUpdRequest
		{
			FirstName = firstName,
			LastName = lastName,
			Email = email
		};
		var userAddedRes = await _userCommandService.Create(dto);
		if (!userAddedRes.IsSuccess)
			throw new System.Exception($"Can't create '{firstName} {lastName}' user");
		await SaveChanges();

		return userAddedRes.Value;
	}
}