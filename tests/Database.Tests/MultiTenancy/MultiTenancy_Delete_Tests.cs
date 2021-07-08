using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

using Xunit;

namespace Raven.Yabt.Database.Tests.MultiTenancy
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class MultiTenancy_Delete_Tests : MultiTenancyTestsBase
	{
		[Fact]
		public async Task Delete_My_Entity_Works()
		{
			// GIVEN a ticket created by the current user
			var ticket = await CreateMySampleTicket();
			
			// WHEN the ticket is loaded and then deleted
			DbSession.Delete(ticket);
			await DbSession.SaveChangesAsync();
			
			// THEN the ticket is NOT present in the DB
			//		`ExistsAsync` returns `false`
			var ticketExists = await DbSession.Advanced.ExistsAsync(ticket.Id);
			Assert.False(ticketExists);
			//		`LoadAsync` returns `null`
			var loadedTicket = await DbSession.LoadAsync<BacklogItemTask>(ticket.Id);
			Assert.Null(loadedTicket);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task Delete_Not_My_Entity_Fails(bool throwExceptionOnWrongTenant)
		{
			ThrowExceptionOnWrongTenant = throwExceptionOnWrongTenant;

			// GIVEN a someone else's ticket
			var ticket = await CreateNotMySampleTicket();
			
			// WHEN delete the ticket
			async Task DeleteTicketFunc()
				{
					DbSession.Delete(ticket);
					await DbSession.SaveChangesAsync();
				}
			if (!throwExceptionOnWrongTenant) 
				await DeleteTicketFunc(); 

			// THEN it fails
			if (throwExceptionOnWrongTenant)
				await Assert.ThrowsAsync<ArgumentException>(DeleteTicketFunc);

			//		and the ticket remains in the DB  
			var ticketExists = await DbSession.Advanced.ExistsAsync(ticket.Id);
			Assert.True(ticketExists);
		}
	}
}
