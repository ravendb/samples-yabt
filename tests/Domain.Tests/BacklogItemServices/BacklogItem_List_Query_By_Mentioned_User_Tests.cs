using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_List_Query_By_Mentioned_User_Tests : ConfigureTestEnvironment, ICurrentUserResolver, IUserReferenceResolver
{
	private readonly IUserCommandService _userCommandService;
	private readonly IBacklogItemCommandService _backlogItemCommandService;
	private readonly IBacklogItemCommentCommandService _commentCommandService;
	private readonly IBacklogItemListQueryService _queryBacklogItemService;

	private UserReference _currentUser = new UserReference { Id = "99-A", Name = "U. Unknown", FullName = "Unknown Unknown" };
	private readonly TestUser[] _allUsers = 
	{
		new TestUser("Homer", "Simpson", "homer@gmail.com"),
		new TestUser("Edna", "Krabappel", "edna@gmail.com"), 
		new TestUser("Ned", "Flanders", "ned@gmail.com")
	};

	public BacklogItem_List_Query_By_Mentioned_User_Tests()
	{
		_userCommandService = Container.GetService<IUserCommandService>()!;
		_backlogItemCommandService = Container.GetService<IBacklogItemCommandService>()!;
		_commentCommandService = Container.GetService<IBacklogItemCommentCommandService>()!;
		_queryBacklogItemService = Container.GetService<IBacklogItemListQueryService>()!;
	}

	[Theory]
	[InlineData("Test @EdnaKrabappel bla", new [] {"Edna Krabappel"})]
	[InlineData("Test @EdnaKrabappel and @NedFlanders bla", new [] {"Ned Flanders", "Edna Krabappel"})]
	[InlineData("Test @EdnaKrabappel and@NedFlanders bla",  new [] {"Edna Krabappel"})]
	[InlineData("Test @EdnaKrabappel and @NedFlandersbla",  new [] {"Edna Krabappel"})]
	private async Task BacklogItems_Can_Be_Queried_By_Mentioned_Users(string message, string[] mentionedUsers)
	{
		// GIVEN 3 users
		var refUsers = await CreateTestUsers();
		// and 2 tickets
		var ticketRef = await CreateSampleBug();
		await CreateSampleBug();

		// WHEN adding a new comment
		await _commentCommandService.Create(ticketRef.Id!, message);
		await SaveChanges();
			
		// THEN 
		// the ticket can be queried by the mentioned users
		foreach (var referencedUser in mentionedUsers)
		{
			var user = refUsers.Single(u => u.FullName == referencedUser);
			_currentUser = user;
				
			var tickets = await _queryBacklogItemService.GetList(new BacklogItemListGetRequest { CurrentUserRelation = CurrentUserRelations.MentionsOf });
			Assert.Equal(1, tickets.TotalRecords);
		}
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		services.AddScoped<IUserReferenceResolver>(_ => this);
		services.AddScoped<ICurrentUserResolver>(_ => this);
	}

	public Task<UserReference> GetCurrentUserReference() => Task.FromResult(_currentUser);
	public Task<UserReference?> GetReferenceById(string id) => throw new NotImplementedException();
	public string GetCurrentUserId() => _currentUser.Id ?? "";

	private async Task<BacklogItemReference> CreateSampleBug()
	{
		var dto = new BugAddUpdRequest
		{
			Title = "Test Bug",
			Severity = BugSeverity.Critical,
			Priority = BugPriority.P1
		};
		var ticketAddedRef = await _backlogItemCommandService.Create(dto);
		if (!ticketAddedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");
		await SaveChanges();

		return ticketAddedRef.Value;
	}

	private async Task<UserReference[]> CreateTestUsers()
	{	
		foreach (var user in _allUsers)
		{
			var userRef = (await _userCommandService.Create(user)).Value;
			user.UpdateFromUserReference(userRef);
				
		}
		return _allUsers.Select(u => u.GetUserReference()).ToArray();
	}

	private class TestUser: UserAddUpdRequest
	{
		public string? Id { get; private set; }
		public string? Name { get; private set; }
		public string? FullName { get; private set; }

		public TestUser(string firstName, string lastName, string email)
		{
			FirstName = firstName;
			LastName = lastName;
			Email = email;
		}

		public void UpdateFromUserReference(UserReference userRef)
		{
			Id = userRef.Id;
			Name = userRef.Name;
			FullName = userRef.FullName;
		}
		public UserReference GetUserReference() => new UserReference
		{
			Id = Id,
			Name = Name??"",
			FullName = FullName??""
		};
	}
}