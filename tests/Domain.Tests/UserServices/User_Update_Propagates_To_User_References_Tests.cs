using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.UserServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class User_Update_Propagates_To_User_References_Tests : ConfigureTestEnvironment
{
	private readonly IUserCommandService _userCommandService;
	private readonly IBacklogItemCommandService _backlogCommandService;
	private readonly IBacklogItemByIdQueryService _backlogQueryService;
	private readonly IBacklogItemCommentCommandService _commentCommandService;
		
	private ICurrentUserResolver _currentUserResolver = null!;	// Initialised in 'ConfigureIocContainer()'
	private string _currentUserId = null!;						// Must be initialised as the 1st step in each test

	public User_Update_Propagates_To_User_References_Tests()
	{
		_userCommandService = Container.GetService<IUserCommandService>()!;
		_backlogCommandService = Container.GetService<IBacklogItemCommandService>()!;
		_backlogQueryService = Container.GetService<IBacklogItemByIdQueryService>()!;
		_commentCommandService = Container.GetService<IBacklogItemCommentCommandService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		_currentUserResolver = Substitute.For<ICurrentUserResolver>();
		_currentUserResolver.GetCurrentUserId().Returns(_ => _currentUserId);
		services.AddScoped(_ => _currentUserResolver);
	}

	#region DELETE User -----------------------------------------
		
	[Fact]
	private async Task Deleting_User_Clears_UserId_In_BacklogItem_Modified_Collection()
	{
		// GIVEN a user and a backlog item created by him
		var userRef = await CreateSampleUser();
		_currentUserId = userRef.Id!;
		var backlogItemRef = await CreateBacklogItem();

		// WHEN the user gets deleted
		await _userCommandService.Delete(userRef.Id!);
		await SaveChanges();

		// THEN all the references to the deleted user lack of its ID
		var item = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value;
		Assert.Null(item.Created.ActionedBy.Id);
		Assert.Null(item.LastUpdated.ActionedBy.Id);
		// and the user's name remains
		Assert.Equal(userRef.Name, item.Created.ActionedBy.Name);
		Assert.Equal(userRef.Name, item.LastUpdated.ActionedBy.Name);
	}
		
	[Fact]
	private async Task Deleting_Users_Clears_BacklogItem_Assignee()
	{
		// GIVEN a user and a backlog item assigned to the user
		_currentUserId = (await CreateSampleUser()).Id!;
		var userRef = await CreateSampleUser("Bart");
		var backlogItemRef = await CreateBacklogItem();
		await _backlogCommandService.AssignToUser(backlogItemRef.Id!, userRef.Id!);

		// WHEN the user gets deleted
		await _userCommandService.Delete(userRef.Id!);
		await SaveChanges();
			
		// THEN the assignee of the Backlog Item is NULL
		var item = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value;
		Assert.Null(item.Assignee);
	}

	[Fact]
	private async Task Deleting_User_Clears_UserIds_In_Backlog_Comments()
	{
		// GIVEN a backlog item with 2 comments with references to a user
		_currentUserId = (await CreateSampleUser("Marge")).Id!;
		var userRef = await CreateSampleUser();
		var backlogItemRef = await CreateBacklogItemWithACommentAndUserReference(userRef, userRef);
			
		// WHEN the user gets deleted
		await _userCommandService.Delete(userRef.Id!);
		await SaveChanges();

		// THEN all the comments have been cleaned from references to the deleted user
		var comments = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value.Comments;
		Assert.Empty(comments!.Where(c => c.MentionedUserIds?.Values.Contains(userRef.Id!) == true));
	}

	[Fact]
	private async Task Deleting_User_Clears_UserIds_In_Backlog_Comment_Authors()
	{
		// GIVEN a user and a backlog item assigned to the user
		var initialUserId = (await CreateSampleUser("Marge")).Id!;
		_currentUserId = initialUserId;
		var userRef = await CreateSampleUser();
		_currentUserId = userRef.Id!;
		var backlogItemRef = await CreateBacklogItemWithAComment();
		_currentUserId = initialUserId;
			
		// WHEN the user gets deleted
		await _userCommandService.Delete(userRef.Id!);
		await SaveChanges();

		// THEN the Author of the comment remains the name, but the ID is NULL
		var comment = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value.Comments!.First();
		Assert.Null(comment.Author.Id);
		Assert.Equal("Homer Simpson", comment.Author.FullName);
	}
	#endregion / DELETE User ------------------------------------

	#region UPDATE User gets reflected in Comments --------------
		
	[Fact]
	private async Task Updating_Users_Name_Updates_Backlog_Comment_Author()
	{
		// GIVEN a backlog item with a comment
		_currentUserId = (await CreateSampleUser("Marge")).Id!;
		var backlogItemRef = await CreateBacklogItemWithAComment();

		// WHEN the user changes its name
		var updatedRef = await UpdateUser(_currentUserId, "Bart", "Simpson");

		// THEN the comment's author has the new name
		var item = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value;
		Assert.Equal(updatedRef.FullName, item.Comments![0].Author.FullName);
	}

	[Fact]
	private async Task Updating_Users_Name_Updates_References_In_Backlog_Comments()
	{
		// GIVEN a backlog item with 2 comments with references to a user
		_currentUserId = (await CreateSampleUser("Marge")).Id!;
		var userRef = await CreateSampleUser();
		var backlogItemRef = await CreateBacklogItemWithACommentAndUserReference(userRef, userRef);

		// WHEN the user changes its name
		var updatedRef = await UpdateUser(userRef.Id!, "Bart", "Simpson");

		// THEN all the references  
		var comments = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value.Comments;
		// ...still exist
		Assert.Equal(2, comments!.Count(c => c.MentionedUserIds?.Values.Contains(userRef.Id, StringComparer.InvariantCultureIgnoreCase) == true));
		// ...'MentionedUserIds' is updated to have the new name
		Assert.Equal(new [] { updatedRef.MentionedName, updatedRef.MentionedName }, comments!.SelectMany(c => c.MentionedUserIds?.Keys ?? Array.Empty<string>()));
		// ...text references are updated to the new name
		Assert.Equal(0, comments!.Count(c => c.Message.Contains(userRef.MentionedName)));
		Assert.Equal(2, comments!.Count(c => c.Message.Contains(updatedRef.MentionedName)));
	}
		
	#endregion / UPDATE User gets reflected in Comments ---------
		
	#region UPDATE User gets reflected in Backlog Item ----------
		
	[Fact]
	private async Task Updating_Users_Name_Updates_BacklogItem_Assignee()
	{
		// GIVEN a user and a backlog item assigned to the user
		_currentUserId = (await CreateSampleUser()).Id!;
		var userRef = await CreateSampleUser("Bart");
		var backlogItemRef = await CreateBacklogItem();
		await _backlogCommandService.AssignToUser(backlogItemRef.Id!, userRef.Id!);

		// WHEN the user changes its name
		var updatedRef = await UpdateUser(userRef.Id!, "Marge", "Simpson");

		// THEN the assignee of the Backlog Item has the new name
		var item = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value;
		Assert.Equal(updatedRef.FullName, item.Assignee!.FullName);
	}
		
	[Fact]
	private async Task Updating_Users_Name_Updates_References_In_BacklogItem_Modified_Collection()
	{
		// GIVEN a user and a backlog item created by him
		var userRef = await CreateSampleUser();
		_currentUserId = userRef.Id!;
		var backlogItemRef = await CreateBacklogItem();

		// WHEN the user changes its name
		var updatedRef = await UpdateUser(userRef.Id!, "Marge", "Simpson");

		// THEN all the references to the updated user have the new name
		var item = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value;
		Assert.Equal(updatedRef.FullName, item.Created.ActionedBy.FullName);
		Assert.Equal(updatedRef.FullName, item.LastUpdated.ActionedBy.FullName);
	}

	[Fact]
	private async Task Updating_Users_Name_Doesnt_MessUp_Other_References_In_Backlog_Items()
	{
		// GIVEN 2 users and a backlog item changed by both
		var homerRef = await CreateSampleUser();
		var nedRef = await CreateSampleUser("Ned", "Flanders");
		_currentUserId = homerRef.Id!;
		var backlogItemRef = await CreateBacklogItem();
		_currentUserId = nedRef.Id!;
		await _backlogCommandService.AssignToUser(backlogItemRef.Id!, nedRef.Id!);

		// WHEN the one user changes its name
		var updatedRef = await UpdateUser(homerRef.Id!, "Marge", "Simpson");

		// THEN only "Created" property has the references updated
		var item = (await _backlogQueryService.GetById(backlogItemRef.Id!)).Value;
		Assert.Equal(updatedRef.FullName, item.Created.ActionedBy.FullName);
		// and "LastUpdated" property hasn't changed
		Assert.Equal(nedRef.FullName, item.LastUpdated.ActionedBy.FullName);
	}
	#endregion / UPDATE User gets reflected in Backlog Item -----

	#region Auxiliary methods -----------------------------------
		
	private async Task<UserReference> CreateSampleUser(string firstName = "Homer", string lastName = "Simpson")
	{
		var dto = new UserAddUpdRequest
		{
			FirstName = firstName,
			LastName = lastName
		};
		var userAddedRef = await _userCommandService.Create(dto);
		if (!userAddedRef.IsSuccess)
			throw new Exception("Failed to create a user");

		await SaveChanges();

		return userAddedRef.Value;
	}

	private async Task<UserReference> UpdateUser(string id, string firstName, string lastName)
	{
		var dto = new UserAddUpdRequest { FirstName = firstName, LastName = lastName };
		var updatedRef = await _userCommandService.Update(id, dto);
		if (!updatedRef.IsSuccess)
			throw new Exception("Failed to update a user");

		await SaveChanges();

		return updatedRef.Value;
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

	private async Task<BacklogItemReference> CreateBacklogItemWithAComment()
	{
		var ticket = await CreateBacklogItem();

		var createdComment = await _commentCommandService.Create(ticket.Id!, "Test bla");
		if (!createdComment.IsSuccess)
			throw new Exception("Failed to create a comment on a backlog item");
		await SaveChanges();
			
		return ticket;
	}

	private async Task<BacklogItemReference> CreateBacklogItemWithACommentAndUserReference(params UserReference[] referredUser)
	{
		var ticket = await CreateBacklogItem();

		foreach (var reference in referredUser)
		{
			var createdComment = await _commentCommandService.Create(ticket.Id!, $"Test @{reference.MentionedName} bla");
			if (!createdComment.IsSuccess)
				throw new Exception("Failed to create a comment on a backlog item");
		}
		await SaveChanges();
			
		return ticket;
	}
	#endregion / Auxiliary methods ------------------------------
}