using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

namespace Raven.Yabt.Database.Tests.MultiTenancy
{
	public abstract class MultiTenancyTestsBase : ConfigureTestEnvironment
	{
		private const string MyTenantId = "1-A";
		private const string NotMyTenantId = "2-A";

		private bool _isMyTenantFlag = true;

		protected override string GetCurrentTenantId() => _isMyTenantFlag ? MyTenantId : NotMyTenantId;

		protected Task<BacklogItemTask> CreateMySampleTicket() => CreateSampleTicket();

		protected async Task<BacklogItemTask> CreateNotMySampleTicket()
		{
			_isMyTenantFlag = false;
			var task = await CreateSampleTicket();
			_isMyTenantFlag = true;
			return task;
		}
		
		private async Task<BacklogItemTask> CreateSampleTicket()
		{
			var entity = new BacklogItemTask { Title = "Test task" };
			await DbSession.StoreAsync(entity);
			await DbSession.SaveChangesAsync();

			return entity;
		}
	}
}
