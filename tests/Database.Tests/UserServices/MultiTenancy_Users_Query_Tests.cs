using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Database.Models.Users.Indexes;

using Xunit;

namespace Raven.Yabt.Database.Tests.UserServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MultiTenancy_UserServices_Query_Tests : ConfigureTestEnvironment
{
	[Fact]
	public async Task Querying_Users_By_Entity_Filters_Result_By_Tenant()
	{
		// GIVEN 2 users with different TenantIDs
		var myUser = await CreateUserUnderCurrentTenant();
		await CreateUserUnderAnotherTenant();
			
		// WHEN query all users
		var users = (await DbSession.Query<User>().ToArrayAsync())
		            .Select(t => t.Id).ToArray();
			
		// THEN the list has the only the user of the current tenant
		Assert.Equal(new [] { myUser.Id }, users);
	}

	[Fact]
	public async Task Querying_Users_By_Index_Filters_Result_By_Tenant()
	{
		// GIVEN 2 users with different TenantIDs
		var myUser = await CreateUserUnderCurrentTenant();
		await CreateUserUnderAnotherTenant();
			
		// WHEN query all users
		var users = await DbSession.Query<User,Users_ForList>().ToArrayAsync();
		var userIds = users.Select(t => t.Id).ToArray();
			
		// THEN the list has only the user of the current tenant
		Assert.Equal(new [] { myUser.Id }, userIds);
	}
	
	[Fact]
	public async Task Querying_Users_By_Users_ForList_Index_And_Selecting_Fields_Filters_Result_By_Tenant()
	{
		// GIVEN 2 users with different TenantIDs
		var myUser = await CreateUserUnderCurrentTenant();
		await CreateUserUnderAnotherTenant();
			
		// WHEN query all users
		var query = DbSession.Query<User,Users_ForList>().Select(t => t.Id);
		var userIds = await query.ToArrayAsync();
			
		// THEN the list has only the user of the current tenant
		Assert.Equal(new [] { myUser.Id }, userIds);
	}
	
	[Fact]
	public async Task Querying_Users_By_Users_Mentions_Index_And_Selecting_Fields_Filters_Result_By_Tenant()
	{
		// GIVEN 2 users with different TenantIDs
		var myUser = await CreateUserUnderCurrentTenant();
		await CreateUserUnderAnotherTenant();
			
		// WHEN query all users
		var query = DbSession.Query<MentionedUsersIndexed, Users_Mentions>().Select(t => t.Id);
		var userIds = await query.ToArrayAsync();
			
		// THEN the list has only the user of the current tenant
		Assert.Equal(new [] { myUser.Id }, userIds);
	}
	
	private Task<User> CreateUserUnderCurrentTenant(Action<User>? setExtraPropertiesAction = null) 
		=> CreateSampleUser(setExtraPropertiesAction);

	private async Task<User> CreateUserUnderAnotherTenant(Action<User>? setExtraPropertiesAction = null)
	{
		IsMyTenantFlag = false;
		var task = await CreateSampleUser(setExtraPropertiesAction);
		IsMyTenantFlag = true;
		return task;
	}
		
	private async Task<User> CreateSampleUser(Action<User>? setExtraPropertiesAction = null)
	{
		var entity = new User { FirstName = "Homer", LastName = "Simpson" };
		setExtraPropertiesAction?.Invoke(entity);
			
		await DbSession.StoreAsync(entity);
		await DbSession.SaveChangesAsync();

		return entity;
	}
}