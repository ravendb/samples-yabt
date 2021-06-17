using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

using Xunit;

namespace Raven.Yabt.Database.Tests.MultiTenancy
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class MultiTenancy_Create_And_Load_Tests : MultiTenancyTestsBase
	{
		[Fact]
		public async Task Created_Entity_Has_TenantId_Injected()
		{
			// GIVEN an empty DB
			
			// WHEN create a task
			var createdTicket = await CreateMySampleTicket();
			
			// THEN it the current Tenant ID injected
			DbSession.Advanced.Clear();
			var validatedTicket = await DbSession.LoadAsync<BacklogItemTask>(createdTicket.Id);
			Assert.Equal(GetCurrentTenantId(), validatedTicket.TenantId);
		}

		[Fact]
		public async Task Loading_Entity_With_Wrong_TenantId_Fails()
		{
			// GIVEN a ticket with a different TenantID
			var ticket = await CreateNotMySampleTicket();
			
			// WHEN load the ticket by ID
			DbSession.Advanced.Clear();
			Task LoadTicket() => DbSession.LoadAsync<BacklogItemTask>(ticket.Id);
			
			// THEN it throws an exception
			var exception = await Assert.ThrowsAsync<InvalidOperationException>(LoadTicket);
			Assert.True(exception.InnerException is InvalidTenantException);
		}
	}
}
