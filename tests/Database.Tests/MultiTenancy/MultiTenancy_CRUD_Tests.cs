using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Raven.Yabt.Database.Models.BacklogItems;

using Xunit;

namespace Raven.Yabt.Database.Tests.MultiTenancy
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class MultiTenancy_CRUD_Tests : ConfigureTestEnvironment
	{
		private const string MyTenantId = "1-A";
		private const string NotMyTenantId = "2-A";

		private bool _isMyTenantFlag = true;
		
		[Fact]
		public async Task Created_Entity_Has_TenantId_Injected()
		{
			// GIVEN an empty DB
			
			// WHEN create a task
			var taskId = await CreateMySampleTicket();
			
			// THEN it the current Tenant ID injected
			var task = await DbSession.LoadAsync<BacklogItemTask>(taskId);
			Assert.Equal(GetCurrentTenantId(), task.TenantId);
		}

		[Fact]
		public async Task DeleteById_My_Entity_Works()
		{
			// GIVEN a task
			var taskId = await CreateMySampleTicket();
			
			// WHEN delete the task
			DbSession.Delete(taskId);
			await DbSession.SaveChangesAsync();
			
			// THEN the task is deleted
			var task = await DbSession.LoadAsync<BacklogItemTask>(taskId);
			Assert.Null(task);
		}

		[Fact]
		public async Task DeleteByEntity_My_Entity_Works()
		{
			// GIVEN a task
			var taskId = await CreateMySampleTicket();
			
			// WHEN delete the task
			var task = await DbSession.LoadAsync<BacklogItemTask>(taskId);
			DbSession.Delete(task);
			await DbSession.SaveChangesAsync();
			
			// THEN the task is deleted
			task = await DbSession.LoadAsync<BacklogItemTask>(taskId);
			Assert.Null(task);
		}

		[Fact]
		public async Task DeleteById_Not_My_Entity_Doesnt_Work()
		{
			// GIVEN a someone else's task
			var taskId = await CreateNotMySampleTicket();
			
			// WHEN delete the task
			DbSession.Delete(taskId);
			await DbSession.SaveChangesAsync();
			
			// THEN the task is still there
			var task = await DbSession.LoadAsync<BacklogItemTask>(taskId);
			Assert.NotNull(task);
		}

		protected override string GetCurrentTenantId() => _isMyTenantFlag ? MyTenantId : NotMyTenantId;

		private Task<string> CreateMySampleTicket() => CreateSampleTicket();

		private async Task<string> CreateNotMySampleTicket()
		{
			_isMyTenantFlag = false;
			var taskId = await  CreateSampleTicket();
			_isMyTenantFlag = true;
			return taskId;
		}
		
		private async Task<string> CreateSampleTicket()
		{
			var entity = new BacklogItemTask { Title = "Test task" };
			await DbSession.StoreAsync(entity);
			await DbSession.SaveChangesAsync();

			return entity.Id;
		}
	}
}
