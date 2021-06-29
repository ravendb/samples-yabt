using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Session.Loaders;

namespace Raven.Yabt.Database.Infrastructure
{
	/// <summary>
	///		Special multi-tenanted DB session - a wrapper on <see cref="IAsyncDocumentSession"/>.
	///		`IAsyncDocumentSession` methods that won't support multi-tenancy checks (e.g. Delete(string)) have been omitted
	/// </summary>
	public interface IAsyncTenantedDocumentSession : IDisposable
	{
		IAsyncAdvancedSessionOperations Advanced { get; }

		IAsyncSessionDocumentCounters CountersFor(string documentId);
		IAsyncSessionDocumentCounters CountersFor(object entity);
		
		void Delete<T>(T entity) where T: new();

		Task SaveChangesAsync(CancellationToken token = default);

		Task StoreAsync(object entity, CancellationToken token = default);
		Task StoreAsync(object entity, string changeVector, string id, CancellationToken token = default);
		Task StoreAsync(object entity, string id, CancellationToken token = default);

		Task<T?> LoadAsync<T>(string id, CancellationToken token = default);
		Task<T?> LoadAsync<T>(string id, Action<IIncludeBuilder<T>> includes, CancellationToken token = default);
		Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, Action<IIncludeBuilder<T>> includes, CancellationToken token = default);
		Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, CancellationToken token = default);

		IRavenQueryable<T> Query<T>(string? indexName = null, string? collectionName = null, bool isMapReduce = false);
		IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new();

		bool HasChanges();
	}
}
