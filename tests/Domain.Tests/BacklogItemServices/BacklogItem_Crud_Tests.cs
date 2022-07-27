using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using DomainResults.Common;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_Crud_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemByIdQueryService _queryService;

	private readonly UserReference _currentUser = new (){ Id = "1", Name = "H. Simpson", FullName = "Homer Simpson" };

	public BacklogItem_Crud_Tests()
	{
		_commandService = Container.GetService<IBacklogItemCommandService>()!;
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
	private async Task Added_Bug_Can_Be_Queried_By_Id()
	{
		// GIVEN an empty DB

		// WHEN adding a new ticket
		var ticketRef = await CreateSampleBug();

		// THEN 
		// The returned ID of the newly created ticket gets returned
		Assert.NotNull(ticketRef);

		// the ticket appears in the DB
		var ticket = await _queryService.GetById(ticketRef.Id!);
		Assert.True(ticket.IsSuccess);
		Assert.Equal(ticketRef.Name, ticket.Value.Title);
	}

	[Fact]
	private async Task Updated_Bug_Properties_Get_Persisted()
	{
		// GIVEN a ticket
		var ticketRef = await CreateSampleBug();

		// WHEN changing the title of the ticket
		var dto = new BugAddUpdRequest
		{
			Title = "Test Bug (Updated)",
			Severity = BugSeverity.Low,
			Priority = BugPriority.P1
		};
		var ticketUpdatedRef = await _commandService.Update(ticketRef.Id!, dto);
		await SaveChanges();

		// THEN 
		// The returned ID of the updated ticket hasn't changed
		Assert.True(ticketUpdatedRef.IsSuccess);
		Assert.Equal(ticketRef.Id, ticketUpdatedRef.Value.Id);

		// the new ticket's properties appear in the DB
		var ticket = await _queryService.GetById(ticketRef.Id!);
		Assert.True(ticket.IsSuccess);
		var bug = ticket.Value as BugGetResponse;
		Assert.NotNull(bug);
		Assert.Equal(dto.Title, bug!.Title);
		Assert.Equal(dto.Severity, bug.Severity);
		Assert.Equal(dto.Priority, bug.Priority);
	}

	[Fact]
	private async Task Deleted_Bug_Disappears_From_Db()
	{
		// GIVEN a ticket
		var ticketRef = await CreateSampleBug();

		// WHEN deleting the ticket
		var ticketDeletedRef = await _commandService.Delete(ticketRef.Id!);
		await SaveChanges();

		// THEN 
		// The returned ID of the deleted ticket is correct
		Assert.True(ticketDeletedRef.IsSuccess);
		Assert.Equal(ticketRef.Id, ticketDeletedRef.Value.Id);

		// the ticket disappears from the DB
		var ticket = await _queryService.GetById(ticketRef.Id!);
		Assert.Equal(DomainOperationStatus.NotFound, ticket.Status);
	}

	[Fact]
	private async Task Set_New_Status_Get_Persisted()
	{
		// GIVEN a ticket with 'Open' status
		var (id, _) = await CreateSampleBug();

		// WHEN changing the status to 'Closed'
		await _commandService.SetState(id!, BacklogItemState.Closed);
		await SaveChanges();
			
		// THEN 
		// The new state gets persisted
		var (ticket, _) = await _queryService.GetById(id!);
		Assert.Equal(BacklogItemState.Closed, ticket.State);
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