using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;

using Xunit;

namespace Raven.Yabt.Database.Tests.MultiTenancy
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class MultiTenancy_Query_Tests : MultiTenancyTestsBase
	{
		[Fact]
		public async Task Querying_By_Entity_Filters_Result_By_Tenant()
		{
			// GIVEN 2 tickets with different TenantIDs
			var ticketMy = await CreateMySampleTicket();
			await CreateNotMySampleTicket();
			
			// WHEN query all tickets
			var tickets = (await DbSession.Query<BacklogItemTask>()
			                              .ToArrayAsync()
			                     ).Select(t => t.Id).ToArray();
			
			// THEN the list has only my ticket(s)
			Assert.Equal(new [] { ticketMy.Id }, tickets);
		}

		[Fact]
		public async Task Querying_By_Index_Filters_Result_By_Tenant()
		{
			// GIVEN 2 tickets with different TenantIDs
			var ticketMy = await CreateMySampleTicket();
			await CreateNotMySampleTicket();
			
			// WHEN query all tickets
			var tickets = await DbSession.Query<BacklogItem,BacklogItems_ForList>().ToArrayAsync();
			var ticketIds = tickets.Select(t => t.Id).ToArray();
			
			// THEN the list has only my ticket(s)
			Assert.Equal(new [] { ticketMy.Id }, ticketIds);
		}

		[Fact]
		public async Task Querying_By_Entity_And_Selecting_Fields_Filters_Result_By_Tenant()
		{
			// GIVEN 2 tickets with different TenantIDs
			var ticketMy = await CreateMySampleTicket();
			await CreateNotMySampleTicket();
			
			// WHEN query all tickets
			var query = DbSession.Query<BacklogItemTask>().Select(t => t.Id);
			var tickets = await query.ToArrayAsync();
			
			// THEN the list has only my ticket(s)
			Assert.Equal(new [] { ticketMy.Id }, tickets);
		}

		[Fact]
		public async Task Querying_By_Index_And_Selecting_Fields_Filters_Result_By_Tenant()
		{
			// GIVEN 2 tickets with different TenantIDs
			var ticketMy = await CreateMySampleTicket();
			await CreateNotMySampleTicket();
			
			// WHEN query all tickets
			var tickets = await DbSession.Query<BacklogItemIndexedForList,BacklogItems_ForList>().Select(t => t.Id).ToArrayAsync();
			
			// THEN the list has only my ticket(s)
			Assert.Equal(new [] { ticketMy.Id }, tickets);
		}
	}
}
