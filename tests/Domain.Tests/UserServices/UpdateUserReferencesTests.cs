using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.UserServices
{
	public class UpdateUserReferencesTests : ConfigureTestEnvironment
	{
		private readonly IUserCommandService _userCommandService;
		private readonly IBacklogItemCommandService _backlogCommandService;
		private readonly IBacklogItemCommentCommandService _commentCommandService;
		private readonly IBacklogItemByIdQueryService _backlogQueryService;
			
		private ICurrentUserResolver _currentUserResolver;	// Initialised in 'ConfigureIocContainer()'
		private string _currentUserId = null!;				// Must be initialised as the 1st step in each test

		public UpdateUserReferencesTests()
		{
			_userCommandService = Container.GetService<IUserCommandService>();
			_backlogCommandService = Container.GetService<IBacklogItemCommandService>();
			_commentCommandService = Container.GetService<IBacklogItemCommentCommandService>();
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
		private async Task Deleting_User_Clears_UserIds_In_Backlog_Comments()
		{
			// GIVEN a backlog item with 2 comments with references to a user
			_currentUserId = (await CreateSampleUser("Marge")).Id!;
			var userRef = await CreateSampleUser();
			var backlogItemRef = await CreateBacklogItemWithAComment(userRef, userRef);

			// WHEN the user gets deleted
			await _userCommandService.Delete(userRef.Id!);
			await SaveChanges();

			// THEN all the comments have been cleaned from references to the deleted user
			var comments = await _backlogQueryService.GetBacklogItemComments(backlogItemRef.Id!, new BacklogItemCommentListGetRequest());
			Assert.Empty(comments.Entries.Where(c => c.MentionedUserIds.Values.Contains(userRef.Id)));
		}

		[Fact]
		private async Task Updating_Users_Name_Updates_References_In_Backlog_Items()
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
		private async Task Updating_Users_Name_Updates_References_In_Backlog_Comments()
		{
			// GIVEN a backlog item with 2 comments with references to a user
			_currentUserId = (await CreateSampleUser("Marge")).Id!;
			var userRef = await CreateSampleUser();
			var backlogItemRef = await CreateBacklogItemWithAComment(userRef, userRef);

			// WHEN the user changes its name
			var updatedRef = await UpdateUser(userRef.Id!, "Bart", "Simpson");

			// THEN all the references  
			var comments = await _backlogQueryService.GetBacklogItemComments(backlogItemRef.Id!, new BacklogItemCommentListGetRequest());
			// ...still exist
			Assert.Equal(2, comments.Entries.Count(c => c.MentionedUserIds.Values.Contains(userRef.Id, StringComparer.InvariantCultureIgnoreCase)));
			// ...'MentionedUserIds' is updated to have the new name
			Assert.Equal(new [] { updatedRef.MentionedName, updatedRef.MentionedName }, comments.Entries.SelectMany(c => c.MentionedUserIds.Keys));
			// ...text references are updated to the new name
			Assert.Equal(0, comments.Entries.Count(c => c.Message.Contains(userRef.MentionedName)));
			Assert.Equal(2, comments.Entries.Count(c => c.Message.Contains(updatedRef.MentionedName)));
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

		private async Task<BacklogItemReference> CreateBacklogItemWithAComment(params UserReference[] referredUser)
		{
			var ticket = await CreateBacklogItem();

			foreach (var reference in referredUser)
			{
				var createdComment = await _commentCommandService.Create(
					ticket.Id!,
					new CommentAddUpdRequest { Message = $"Test @{reference.MentionedName} bla" }
				);
				if (!createdComment.IsSuccess)
					throw new Exception("Failed to create a comment on a backlog item");
			}
			await SaveChanges();
			
			return ticket;
		}
	}
}
