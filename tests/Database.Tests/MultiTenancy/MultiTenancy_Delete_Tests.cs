using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Xunit;

namespace Raven.Yabt.Database.Tests.MultiTenancy
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class MultiTenancy_Delete_Tests : MultiTenancyTestsBase
	{
		[Fact]
		public async Task DeleteByEntity_My_Entity_Works()
		{
			// GIVEN a ticket created by the current user
			var ticket = await CreateMySampleTicket();
			
			// WHEN the ticket is loaded and then deleted
			DbSession.Delete(ticket);
			await DbSession.SaveChangesAsync();
			
			// THEN the ticket is NOT present in the DB
			var ticketExists = await DbSession.Advanced.ExistsAsync(ticket.Id);
			Assert.False(ticketExists);
		}

		[Fact]
		public async Task DeleteByEntity_Not_My_Entity_Fails()
		{
			// GIVEN a someone else's ticket
			var ticket = await CreateNotMySampleTicket();
			
			// WHEN delete the ticket
			async Task DeleteTicket()
				{
					DbSession.Delete(ticket);
					await DbSession.SaveChangesAsync();
				}

			// THEN it throws an exception
			await Assert.ThrowsAsync<ArgumentException>(DeleteTicket);
			//		and the ticket remains in the DB  
			DbSession.Advanced.Clear();	// Required due to https://github.com/ravendb/ravendb/issues/12398
			var ticketExists = await DbSession.Advanced.ExistsAsync(ticket.Id);
			Assert.True(ticketExists);
		}
	}
}
