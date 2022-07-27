using System.Diagnostics;
using System.Threading.Tasks;

using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.Infrastructure;

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
		bool hasSaved;
		Stopwatch sw = Stopwatch.StartNew();
		try
		{	// Saving. Note it throws an exception on any error
			hasSaved =await DbSession.SaveChangesAsync();	// Though, Stopwatch is not thread-safe, it's used as a local variable, so must be OK when the context is switched on return 
		}
		finally
		{
			sw.Stop();
		}
		return hasSaved ? sw.ElapsedMilliseconds : 0;
	}
}