using System;
using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

namespace Raven.Yabt.Database.Tests.BacklogItem
{
	public abstract class MultiTenancyTestsBase : ConfigureTestEnvironment
	{
		protected Task<BacklogItemTask> CreateMySampleTicket(Action<BacklogItemTask>? setExtraPropertiesAction = null) 
			=> CreateSampleTicket(setExtraPropertiesAction);

		protected async Task<BacklogItemTask> CreateNotMySampleTicket(Action<BacklogItemTask>? setExtraPropertiesAction = null)
		{
			IsMyTenantFlag = false;
			var task = await CreateSampleTicket(setExtraPropertiesAction);
			IsMyTenantFlag = true;
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
}
