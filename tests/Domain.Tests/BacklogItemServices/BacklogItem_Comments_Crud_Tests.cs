using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_Comments_Crud_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemCommentCommandService _commentCommandService;
	private readonly IBacklogItemByIdQueryService _queryService;

	private readonly UserReference _currentUser = new UserReference { Id = "1", Name = "H. Simpson", FullName = "Homer Simpson" };

	public BacklogItem_Comments_Crud_Tests()
	{
		_commandService = Container.GetService<IBacklogItemCommandService>()!;
		_commentCommandService = Container.GetService<IBacklogItemCommentCommandService>()!;
		_queryService = Container.GetService<IBacklogItemByIdQueryService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		var userResolver = Substitute.For<IUserReferenceResolver>();
		userResolver.GetCurrentUserReference().Returns(_currentUser);
		services.AddScoped(_ => userResolver);
	}

	[Fact]
	private async Task Added_Comment_Can_Be_Queried()
	{
		// GIVEN a bug
		var ticketRef = await CreateSampleBug();

		// WHEN adding a new comment
		var commentRefRes = await _commentCommandService.Create(ticketRef.Id!, "Test");
		await SaveChanges();

		// THEN 
		// the operation is successful
		Assert.True(commentRefRes.IsSuccess);

		// the ticket appears in the DB
		var ticket = (await _queryService.GetById(ticketRef.Id!)).Value;
		Assert.NotNull(ticket);
		// it has 1 comment
		Assert.Equal(1, ticket!.Comments!.Count);
		// all the comment properties are as expected
		var commentRef = commentRefRes.Value;
		var comment = ticket.Comments.Single();
		Assert.Equal(commentRef.CommentId, comment.Id);
		Assert.Equal("Test", comment.Message);
		Assert.Equal(_currentUser.Id, comment.Author.Id);
	}

	[Fact]
	private async Task Updated_Comment_Message_Gets_Persisted()
	{
		// GIVEN a 'bug' with a comment
		var ticketRef = await CreateSampleBug();
		var commentRef = (await _commentCommandService.Create(ticketRef.Id!, "Test")).Value;
		await SaveChanges();

		// WHEN changing the message of the comment
		var newText = "Test (Updated)";
		var commentUpdatedRef = await _commentCommandService.Update(ticketRef.Id!, commentRef.CommentId!, newText);
		await SaveChanges();

		// THEN 
		// the operation is successful
		Assert.True(commentUpdatedRef.IsSuccess);
		Assert.Equal(commentRef.CommentId, commentUpdatedRef.Value.CommentId);

		// the comment has the right message
		var ticket = (await _queryService.GetById(ticketRef.Id!)).Value;
		Assert.NotNull(ticket);
		var comment = ticket!.Comments!.Single();
		Assert.Equal("Test (Updated)", comment.Message);
	}

	[Fact]
	private async Task Deleted_Comment_Disappears_From_Db()
	{
		// GIVEN a 'bug' with a comment
		var (id, _) = await CreateSampleBug();
		var commentRef = (await _commentCommandService.Create(id!, "Test")).Value;
		await SaveChanges();

		// WHEN deleting the comment
		var ticketDeletedRef = await _commentCommandService.Delete(id!, commentRef.CommentId!);
		await SaveChanges();

		// THEN 
		// the operation is successful
		Assert.True(ticketDeletedRef.IsSuccess);

		// the comment gets removed from the ticket
		var ticket = (await _queryService.GetById(id!)).Value;
		Assert.NotNull(ticket);
		Assert.True(ticket.Comments?.Any() != true);
	}

	private async Task<BacklogItemReference> CreateSampleBug()
	{
		var dto = new BugAddUpdRequest
		{
			Title = "Test Bug",
			Severity = BugSeverity.Critical,
			Priority = BugPriority.P1
		};
		var ticketAddedRef = await _commandService.Create(dto);
		if (!ticketAddedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");
		await SaveChanges();

		return ticketAddedRef.Value;
	}
}