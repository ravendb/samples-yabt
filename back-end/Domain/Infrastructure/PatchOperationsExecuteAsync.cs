using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace Raven.Yabt.Domain.Infrastructure
{
    /// <summary>
    ///     Base class for notification handlers to patch data in teh database on saving the main transaction
    /// </summary>
    internal class PatchOperationsExecuteAsync : IPatchOperationsExecuteAsync, IPatchOperationsAddDeferred
    {
        private readonly IAsyncDocumentSession _dbSession;

        /// <summary>
        ///		Collection of deferred patch queries.
        ///		Will be executed after saving data in the main DB session
        /// </summary>
        private readonly List<IndexQuery> _deferredPatchQueries = new List<IndexQuery>();

        public PatchOperationsExecuteAsync(IAsyncDocumentSession dbSession)
        {
            _dbSession = dbSession;
        }

        /// <inheritdoc/>
        public bool AreDeferredPatchesForExecution => _deferredPatchQueries.Any();

        /// <inheritdoc/>
        public async Task SendAsyncDeferredPatchByQueryOperations(bool waitForCompletion = false)
        {
            foreach (var queryIndex in _deferredPatchQueries)
            {
                // The default timeout is not documented. Seems to be around  15 sec
                queryIndex.WaitForNonStaleResultsTimeout = TimeSpan.MaxValue;
                var sentOperation = await _dbSession.Advanced.DocumentStore.Operations.SendAsync(new PatchByQueryOperation(queryIndex));

                if (waitForCompletion)
                    await sentOperation.WaitForCompletionAsync();
            }
            _deferredPatchQueries.Clear();
        }

        /// <inheritdoc/>
        public void AddDeferredPatchQuery(IndexQuery patchQuery)
        {
            _deferredPatchQueries.Add(patchQuery);
        }
    }
}
