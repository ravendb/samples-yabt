using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Client.Documents.Session.Loaders;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Database.Infrastructure
{
	public class AsyncTenantedDocumentSession : IAsyncTenantedDocumentSession
	{
		private IAsyncDocumentSession? _dbSession;
		private readonly IDocumentStore _documentStore;
		private readonly SessionOptions? _sessionOptions; 
		/// <summary>
		///		The current Tenant ID resolver
		/// </summary>
		/// <remarks>
		///		Don't store a resolved tenant ID, as the session can be instantiated before the tenant can be resolved 
		/// </remarks>
		private readonly Func<string> _getCurrentTenantIdFunc;

		/// <summary>
		///		Get a DB session on demand. Keep one open session per instance
		/// </summary>
		private IAsyncDocumentSession DbSession => 
			_dbSession ??= _sessionOptions != null 
				? _documentStore.OpenAsyncSession(_sessionOptions) 
				: _documentStore.OpenAsyncSession();

		/// <summary>
		///		Share <seealso cref="IAsyncDocumentSession.Advanced"/> of the embedded session
		/// </summary>
		public IAsyncAdvancedSessionOperations Advanced => DbSession.Advanced;

		public AsyncTenantedDocumentSession(IDocumentStore documentStore, Func<string> getCurrentTenantIdFunc, SessionOptions? options = null)
		{
			_documentStore = documentStore;
			_getCurrentTenantIdFunc = getCurrentTenantIdFunc;
			_sessionOptions = options;
		}
		
		public static IAsyncTenantedDocumentSession Create(IDocumentStore documentStore, Func<string> getCurrentTenantIdFunc, SessionOptions? options = null) =>
			new AsyncTenantedDocumentSession(documentStore, getCurrentTenantIdFunc, options);

		public void Dispose() => _dbSession?.Dispose();

		public IAsyncSessionDocumentCounters CountersFor(string documentId) => DbSession.CountersFor(documentId);

		public IAsyncSessionDocumentCounters CountersFor(object entity) => DbSession.CountersFor(entity);
		
		public void Delete<T>(T entity) where T: new()
		{
			if (IsNotTenantedEntity(entity) || HasCorrectTenant(entity))
				DbSession.Delete(entity);
			else
				throw new ArgumentException("Attempt to delete a record for another tenant");
		}

		public Task SaveChangesAsync(CancellationToken token = default) => DbSession.SaveChangesAsync(token);

		#region StoreAsync [PUBLIC] -------------------------------------------

		public Task StoreAsync(object entity, CancellationToken token = default)
		{
			SetTenantIdOnEntity(entity);

			return DbSession.StoreAsync(entity, token);
		}

		public Task StoreAsync(object entity, string changeVector, string id, CancellationToken token = default)
		{
			SetTenantIdOnEntity(entity);

			return DbSession.StoreAsync(entity, changeVector, id, token);
		}

		public Task StoreAsync(object entity, string id, CancellationToken token = default)
		{
			SetTenantIdOnEntity(entity);

			return DbSession.StoreAsync(entity, id, token);
		}
		#endregion StoreAsync [PUBLIC] ----------------------------------------

		#region LoadAsync [PUBLIC] --------------------------------------------

		public async Task<T?> LoadAsync<T>(string id, CancellationToken token = default)
		{
			var entity = await DbSession.LoadAsync<T>(id, token);

			return IsNotTenantedEntity(entity) || HasCorrectTenant(entity)
				? entity
				: default;
		}

		public async Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, CancellationToken token = default)
		{
			Dictionary<string, T> entities = await DbSession.LoadAsync<T>(ids, token);
			
			return typeof(T).GetInterfaces().Contains(typeof(ITenantedEntity)) 
				? entities.Where(e => HasCorrectTenant(e.Value)).ToDictionary(i => i.Key, i => i.Value) 
				: entities;
		}

		public async Task<T?> LoadAsync<T>(string id, Action<IIncludeBuilder<T>> includes, CancellationToken token = default)
		{
			var entity = await DbSession.LoadAsync(id, includes, token);

			return IsNotTenantedEntity(entity) || HasCorrectTenant(entity)
				 ? entity
				 : default;
		}

		public async Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, Action<IIncludeBuilder<T>> includes, CancellationToken token = default (CancellationToken))
		{
			Dictionary<string, T> entities = await DbSession.LoadAsync(ids, includes, token);
			
			return typeof(T).GetInterfaces().Contains(typeof(ITenantedEntity)) 
				? entities.Where(HasCorrectTenant).ToDictionary(i => i.Key, i => i.Value) 
				: entities;
		}
		#endregion LoadAsync [PUBLIC] -----------------------------------------

		#region Query [PUBLIC] ------------------------------------------------

		public IRavenQueryable<T> Query<T>(string? indexName = null, string? collectionName = null, bool isMapReduce = false)
		{
			var query = DbSession.Query<T>(indexName, collectionName, isMapReduce);

			if (typeof(T).GetInterfaces().Contains(typeof(ITenantedEntity)))
			{
				var tenantId = _getCurrentTenantIdFunc();	// Evaluate tenant separately from the WHERE condition, otherwise LINQ-to-JavaScript conversion fails 
				return query.Where(e => (e as ITenantedEntity)!.TenantId == tenantId);
			}
			return query;
		}

		public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
		{
			var query = DbSession.Query<T, TIndexCreator>();

			var lastArgType = typeof(TIndexCreator).BaseType?.GenericTypeArguments.LastOrDefault();

			if (lastArgType != null && lastArgType.GetInterfaces().Contains(typeof(ITenantedEntity)))
			{
				var tenantId = _getCurrentTenantIdFunc();	// Evaluate tenant separately from the WHERE condition, otherwise LINQ-to-JavaScript conversion fails
				return query.Where(e => (e as ITenantedEntity)!.TenantId == tenantId);
			}
			return query;
		}
		#endregion Query [PUBLIC] ---------------------------------------------

		public bool HasChanges()
		{
			return _dbSession?.Advanced.HasChanges == true
				// Check if there is a PATCH request
				|| _dbSession?.Advanced is InMemoryDocumentSessionOperations { DeferredCommandsCount: > 0 };
		}

		private static bool IsNotTenantedEntity<T>(T entity) => entity is not ITenantedEntity;
		private bool HasCorrectTenant<T>(T entity) => (entity as ITenantedEntity)?.TenantId == _getCurrentTenantIdFunc();

		private void SetTenantIdOnEntity(object entity)
		{
			if (entity is not ITenantedEntity) return;
			
			var entityType = entity.GetType(); 
			entityType.GetProperty(nameof(ITenantedEntity.TenantId))
			          ?.SetValue(entity, _getCurrentTenantIdFunc());
		}
	}
}