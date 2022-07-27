using System;
using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

namespace Raven.Yabt.Database.Tests.BacklogItem;

public abstract class MultiTenancyTestsBase : ConfigureTestEnvironment
{
	protected async Task<BacklogItemTask> CreateMySampleTicketAndKeepItInCache(Action<BacklogItemTask>? setExtraPropertiesAction = null)
	{
		var lastIsMyTenantFlag = IsMyTenantFlag;
		IsMyTenantFlag = true;
		var entity = await CreateSampleTicket(setExtraPropertiesAction);
		IsMyTenantFlag = lastIsMyTenantFlag;
		return entity;
	}

	protected async Task<BacklogItemTask> CreateNotMySampleTicketAndKeepItInCache(Action<BacklogItemTask>? setExtraPropertiesAction = null)
	{
		var lastIsMyTenantFlag = IsMyTenantFlag;
		IsMyTenantFlag = false;
		var task = await CreateSampleTicket(setExtraPropertiesAction);
		IsMyTenantFlag = lastIsMyTenantFlag;
		return task;
	}
		
	private async Task<BacklogItemTask> CreateSampleTicket(Action<BacklogItemTask>? setExtraPropertiesAction = null)
	{
		var entity = new BacklogItemTask { Title = "Test task" };
		setExtraPropertiesAction?.Invoke(entity);
			
		await DbSession.StoreAsync(entity);
		await DbSession.SaveChangesAsync();

		return entity;
	}
}