using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace Raven.Yabt.Domain.Infrastructure
{
    /// <summary>
    ///     Manages a queue of patch requests and executes them against the database on saving of the main transaction
    /// </summary>
    /// <remarks>
    ///		It must be registered as a single instance for the lifetime of the DB session
    /// </remarks>
    internal class PatchOperationsExecuteAsync : IPatchOperationsExecuteAsync, IPatchOperationsAddDeferred
    {
        private readonly IAsyncDocumentSession _dbSession;

        /// <summary>
        ///		Collection of deferred patch queries.
        ///		Will be executed after saving data in the main DB session
        /// </summary>
        private readonly ConcurrentQueue<IndexQuery> _deferredPatchQueries = new();

        public PatchOperationsExecuteAsync(IAsyncDocumentSession dbSession)
        {
            _dbSession = dbSession;
        }

        /// <inheritdoc/>
        public bool AreDeferredPatchesForExecution => _deferredPatchQueries.Any();

        /// <inheritdoc/>
        public async Task SendAsyncDeferredPatchByQueryOperations(bool waitForCompletion = false)
        {
            while (_deferredPatchQueries.TryDequeue(out var queryIndex))
            {
                // The default timeout is not documented. Seems to be around  15 sec
                queryIndex.WaitForNonStaleResultsTimeout = TimeSpan.MaxValue;
                queryIndex.WaitForNonStaleResults = true;
                var sentOperation = await _dbSession.Advanced.DocumentStore.Operations.SendAsync(new PatchByQueryOperation(queryIndex));

                if (waitForCompletion)
                    await sentOperation.WaitForCompletionAsync();
            }
            _deferredPatchQueries.Clear();
        }

        /// <inheritdoc/>
        public void AddDeferredPatchQuery(IndexQuery patchQuery)
        {
            _deferredPatchQueries.Enqueue(patchQuery);
        }
    }
}
