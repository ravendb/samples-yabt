using System.Diagnostics;
using System.Threading.Tasks;

using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.Infrastructure
{
	public interface IDbSessionSavingTimerWrapper
	{
		/// <summary>
		///		Measuring time on saving any changes
		/// </summary>
		Task<long> SaveChangesWithTimerAsync();
	}

	public class DbSessionSavingTimerWrapper : BaseDbService, IDbSessionSavingTimerWrapper
	{
		public DbSessionSavingTimerWrapper(IAsyncTenantedDocumentSession session) : base (session) {}
		
		/// <inheritdoc/>
		public async Task<long> SaveChangesWithTimerAsync()
		{
			if (!DbSession.HasChanges())
				return 0;
			
			Stopwatch sw = Stopwatch.StartNew();
			try
			{	// Saving. Note it throws an exception on any error
				await DbSession.SaveChangesAsync();	// Though, Stopwatch is not thread-safe, it's used as a local variable, so must be OK when the context is switched on return 
			}
			finally
			{
				sw.Stop();
			}
			return sw.ElapsedMilliseconds;
		}
	}
}
