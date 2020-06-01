using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;

namespace Raven.Yabt.Domain.Infrastructure
{
    /// <summary>
    ///     Base class for notification handlers to patch data in teh database on saving the main transaction
    /// </summary>
    internal class PatchOperationsExecuteAsync : IPatchOperationsExecuteAsync
    {
        private readonly IAsyncDocumentSession _dbSession;

        /// <summary>
        ///		Collection of deferred patch queries.
        ///		Will be executed after saving data in the main DB session
        /// </summary>
        private List<IndexQuery> _deferredPatchQueries { get; } = new List<IndexQuery>();

        public PatchOperationsExecuteAsync(IAsyncDocumentSession dbSession)
        {
            _dbSession = dbSession;
        }

        /// <summary>
        ///		Execute deferred queries. Call it after saving data in the main DB session
        /// </summary>
        /// <param name="waitForCompletion"> Wait for execution (use in tests only) </param>
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

        public void AddDeferredPatchQuery(IndexQuery patchQuery)
        {
            _deferredPatchQueries.Add(patchQuery);
        }
    }

    public interface IPatchOperationsExecuteAsync
    {
        void AddDeferredPatchQuery(IndexQuery patchQuery);
        Task SendAsyncDeferredPatchByQueryOperations(bool waitForCompletion = false);
    }
}
