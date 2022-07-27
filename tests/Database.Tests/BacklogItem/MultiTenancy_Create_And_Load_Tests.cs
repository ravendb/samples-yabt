using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

using Xunit;

namespace Raven.Yabt.Database.Tests.BacklogItem;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MultiTenancy_Create_And_Load_Tests : MultiTenancyTestsBase
{
	[Fact]
	public async Task Created_Entity_Has_TenantId_Injected()
	{
		// GIVEN an empty DB
			
		// WHEN create a task
		var createdTicket = await CreateMySampleTicketAndKeepItInCache();
			
		// THEN it the current Tenant ID injected
		var validatedTicket = await DbSession.LoadAsync<BacklogItemTask>(createdTicket.Id);
		Assert.Equal(GetCurrentTenantId(), validatedTicket?.TenantId);
	}
		
	[Fact]
	public async Task Loading_Multiple_Entities_With_Correct_TenantIds_Works()
	{
		// GIVEN 2 tickets
		var ticket1 = await CreateMySampleTicketAndKeepItInCache();
		var ticket2 = await CreateMySampleTicketAndKeepItInCache();
			
		// WHEN load them by IDs
		var loadedTickets = await DbSession.LoadAsync<BacklogItemTask>(new [] {ticket1.Id, ticket2.Id});
			
		// THEN tickets are loaded
		Assert.Equal(2, loadedTickets.Count);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Loading_Entity_With_Wrong_TenantId_Fails(bool throwExceptionOnWrongTenant)
	{
		ThrowExceptionOnWrongTenant = throwExceptionOnWrongTenant;
			
		// GIVEN a ticket with a different TenantID
		var ticket = await CreateNotMySampleTicketAndKeepItInCache();
			
		// WHEN load the ticket by ID
		Task<BacklogItemTask?> LoadTicketFunc() => DbSession.LoadAsync<BacklogItemTask>(ticket.Id);

		var loadedTicket = !throwExceptionOnWrongTenant ? await LoadTicketFunc() : null; 
			
		// THEN the record is not loaded
		if (throwExceptionOnWrongTenant)
			await Assert.ThrowsAsync<ArgumentException>(LoadTicketFunc);
		else
			Assert.Null(loadedTicket);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task Loading_Entity_With_Wrong_TenantIds_Fails(bool throwExceptionOnWrongTenant)
	{
		ThrowExceptionOnWrongTenant = throwExceptionOnWrongTenant;
			
		// GIVEN 2 tickets with a different TenantID
		var ticket1 = await CreateNotMySampleTicketAndKeepItInCache();
		var ticket2 = await CreateNotMySampleTicketAndKeepItInCache();
			
		// WHEN load the ticket by ID
		Task<Dictionary<string, BacklogItemTask>> LoadTicketsFunc() => DbSession.LoadAsync<BacklogItemTask>(new [] {ticket1.Id, ticket2.Id});

		var loadedTickets = !throwExceptionOnWrongTenant ? await LoadTicketsFunc() : new Dictionary<string, BacklogItemTask>(); 
			
		// THEN the records aren't loaded
		if (throwExceptionOnWrongTenant)
			await Assert.ThrowsAsync<ArgumentException>(LoadTicketsFunc);
		else
			Assert.Empty(loadedTickets);
	}
}