using System.Threading.Tasks;

using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace Raven.Yabt.Domain.Infrastructure
{
	/// <summary>
	/// 	Adding patches for deferred executions
	/// </summary>
	public interface IPatchOperationsAddDeferred
	{
		/// <summary>
		/// 	Add a RavenDB patch request for executing after calling <see cref="IAsyncDocumentSession.SaveChangesAsync"/> 
		/// </summary>
		void AddDeferredPatchQuery(IndexQuery patchQuery);
	}

	/// <summary>
	/// 	Executes deferred patches
	/// </summary>
	public interface IPatchOperationsExecuteAsync
	{
		/// <summary>
		/// 	Flag indicating that there are deferred patches for execution
		/// </summary>
		bool AreDeferredPatchesForExecution { get; }
		
		/// <summary>
		///		Execute deferred queries. Call it after saving data in the main DB session
		/// </summary>
		/// <param name="waitForCompletion"> Wait for execution (use in tests only) </param>
		Task SendAsyncDeferredPatchByQueryOperations(bool waitForCompletion = false);
	}	
}
