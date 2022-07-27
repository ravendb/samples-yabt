using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;

using Xunit;

namespace Raven.Yabt.Database.Tests.BacklogItem;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MultiTenancy_Query_Tests : MultiTenancyTestsBase
{
	[Fact]
	public async Task Querying_By_Entity_Filters_Result_By_Tenant()
	{
		// GIVEN 2 tickets with different TenantIDs
		var ticketMy = await CreateMySampleTicketAndKeepItInCache();
		await CreateNotMySampleTicketAndKeepItInCache();
			
		// WHEN query all tickets
		var tickets = (await DbSession.Query<BacklogItemTask>()
		                              .ToArrayAsync()
			).Select(t => t.Id).ToArray();
			
		// THEN the list has only my ticket(s)
		Assert.Equal(new [] { ticketMy.Id }, tickets);
	}

	[Fact]
	public async Task Querying_BacklogItems_By_Index_Filters_Result_By_Tenant()
	{
		// GIVEN 2 tickets with different TenantIDs
		var ticketMy = await CreateMySampleTicketAndKeepItInCache();
		await CreateNotMySampleTicketAndKeepItInCache();
			
		// WHEN query all tickets
		var tickets = await DbSession.Query<Models.BacklogItems.BacklogItem,BacklogItems_ForList>().ToArrayAsync();
		var ticketIds = tickets.Select(t => t.Id).ToArray();
			
		// THEN the list has only my ticket(s)
		Assert.Equal(new [] { ticketMy.Id }, ticketIds);
	}

	[Fact]
	public async Task Querying_Tags_By_Index_Filters_Result_By_Tenant()
	{
		// GIVEN 2 tickets with different TenantIDs but the same tag
		const string tag = "test_tag";
		var tags = new[] { tag };
		await CreateMySampleTicketAndKeepItInCache(t => t.Tags = tags);
		await CreateNotMySampleTicketAndKeepItInCache(t => t.Tags = tags);
			
		// WHEN query all tickets by the tag
		var tagStats = await (
			from t in DbSession.Query<BacklogItemTagsIndexed,BacklogItems_Tags>() 
			where t.Name == tag
			select t
		).ToArrayAsync();
			
		// THEN there is only 1 ticket with the tag
		Assert.Equal(1, tagStats.Single().Count);
	}

	[Fact]
	public async Task Querying_By_Entity_And_Selecting_Fields_Filters_Result_By_Tenant()
	{
		// GIVEN 2 tickets with different TenantIDs
		var ticketMy = await CreateMySampleTicketAndKeepItInCache();
		await CreateNotMySampleTicketAndKeepItInCache();
			
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
		var ticketMy = await CreateMySampleTicketAndKeepItInCache();
		await CreateNotMySampleTicketAndKeepItInCache();
			
		// WHEN query all tickets
		var tickets = await DbSession.Query<BacklogItemIndexedForList,BacklogItems_ForList>().Select(t => t.Id).ToArrayAsync();
			
		// THEN the list has only my ticket(s)
		Assert.Equal(new [] { ticketMy.Id }, tickets);
	}
}