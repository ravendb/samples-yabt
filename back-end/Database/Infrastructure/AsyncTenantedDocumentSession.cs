using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Database.Infrastructure;

/// <inheritdoc />
public class AsyncTenantedDocumentSession : IAsyncTenantedDocumentSession
{
	/// <inheritdoc />
	public bool ThrowExceptionOnWrongTenant { get; }

	#region Fields [PRIVATE] ----------------------------------------------
		
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
	///		Time to wait till indexes catch up after saving
	/// </summary>
	/// <remarks>
	///		Use it in testing to avoid running on stale indexes.
	///		Note that when deferred patches need to be applied then <see cref="IAsyncDocumentSession.SaveChangesAsync"/> will wait for indexes anyway
	/// </remarks>
	private readonly TimeSpan? _waitingForIndexesAfterSave;

	/// <summary>
	///		Get a DB session on demand. Keep one open session per instance
	/// </summary>
	private IAsyncDocumentSession DbSession => _dbSession ??= OpenAsyncDocumentSession();

	private IAsyncDocumentSession? _dbSession;
		
	#endregion / Fields [PRIVATE] -----------------------------------------

	/// <summary>
	///		Constructor.
	/// </summary>
	/// <param name="documentStore"> An instance of the <see cref="IDocumentStore"/> </param>
	/// <param name="getCurrentTenantIdFunc"> The function resolving the current Tenant ID </param>
	/// <param name="waitingForIndexesAfterSave"> [Optional] Time to wait till indexes catch up after saving. Use it in testing to avoid running on stale indexes </param>
	/// <param name="throwExceptionOnWrongTenant"> The flag defining the behaviour on requesting a record of a wrong tenant </param>
	/// <param name="options"> Optional session settings (see <see cref="SessionOptions"/>) </param>
	public AsyncTenantedDocumentSession(IDocumentStore documentStore, Func<string> getCurrentTenantIdFunc, TimeSpan? waitingForIndexesAfterSave = null, bool throwExceptionOnWrongTenant = false, SessionOptions? options = null)
	{
		_documentStore = documentStore;
		_getCurrentTenantIdFunc = getCurrentTenantIdFunc;
		_waitingForIndexesAfterSave = waitingForIndexesAfterSave;
		ThrowExceptionOnWrongTenant = throwExceptionOnWrongTenant;
		_sessionOptions = options;
	}
	public void Dispose() => _dbSession?.Dispose();

	/// <inheritdoc />
	public IAsyncSessionDocumentCounters? CountersFor<T>(T entity) where T: notnull
	{
		if (IsNotTenantedType<T>() || HasCorrectTenant(entity))
			return DbSession.CountersFor(entity);
			
		ThrowArgumentExceptionIfRequired();
		return default;
	}
		
	/// <inheritdoc />
	public void Delete<T>(T entity) where T: notnull
	{
		if (IsNotTenantedType<T>() || HasCorrectTenant(entity))
			DbSession.Delete(entity);
		else
			ThrowArgumentExceptionIfRequired();
	}

	/// <inheritdoc />
	public async Task<bool> SaveChangesAsync(CancellationToken token = default)
	{
		if (!HasChanges()) 
			return false;
			
		await DbSession.SaveChangesAsync(token);
		await SendDeferredPatchByQueryOperationsAsync(token);
		return true;
	}

	/// <inheritdoc />
	public async Task<bool> SaveChangesAsync(bool clearCache, CancellationToken token = default)
	{
		var saved = await SaveChangesAsync(token);
		if (clearCache)
			// Clear all cached entities
			DbSession.Advanced.Clear();
			
		return saved;
	}

	#region StoreAsync [PUBLIC] -------------------------------------------

	/// <inheritdoc />
	public Task StoreAsync<T>(T entity, CancellationToken token = default) where T: notnull
	{
		SetTenantIdOnEntity(entity);

		return DbSession.StoreAsync(entity, token);
	}

	/// <inheritdoc />
	public Task StoreAsync<T>(T entity, string changeVector, string id, CancellationToken token = default) where T: notnull
	{
		SetTenantIdOnEntity(entity);

		return DbSession.StoreAsync(entity, changeVector, id, token);
	}

	/// <inheritdoc />
	public Task StoreAsync<T>(T entity, string id, CancellationToken token = default) where T: notnull
	{
		SetTenantIdOnEntity(entity);

		return DbSession.StoreAsync(entity, id, token);
	}
	#endregion StoreAsync [PUBLIC] ----------------------------------------

	#region LoadAsync [PUBLIC] --------------------------------------------

	/// <inheritdoc />
	public async Task<T?> LoadAsync<T>(string id, CancellationToken token = default)
	{
		var entity = await DbSession.LoadAsync<T>(id, token);

		if (entity == null || IsNotTenantedType<T>() || HasCorrectTenant(entity))
			return entity;
			
		ThrowArgumentExceptionIfRequired();
		return default;
	}

	/// <inheritdoc />
	public async Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, CancellationToken token = default)
	{
		Dictionary<string, T> entities = await DbSession.LoadAsync<T>(ids, token);

		if (IsNotTenantedType<T>())
			return entities;

		var sanitisedEntities = entities
		                        .Where(e => HasCorrectTenant(e.Value))
		                        .ToDictionary(i => i.Key, i => i.Value);
			
		if (sanitisedEntities.Count != entities.Count)
			ThrowArgumentExceptionIfRequired();
			
		return sanitisedEntities;
	}
	#endregion LoadAsync [PUBLIC] -----------------------------------------

	#region Query [PUBLIC] ------------------------------------------------

	/// <inheritdoc />
	public IRavenQueryable<T> Query<T>(string? indexName = null, string? collectionName = null, bool isMapReduce = false)
	{
		var query = DbSession.Query<T>(indexName, collectionName, isMapReduce);

		if (IsNotTenantedType<T>())
			return query;

		// Evaluate tenant separately from the WHERE condition, otherwise LINQ-to-JavaScript conversion fails
		var tenantId = _getCurrentTenantIdFunc(); 
			
		// Add an extra WHERE condition on the current tenant
		return query.Where(e => (e as ITenantedEntity)!.TenantId == tenantId);
	}

	/// <inheritdoc />
	public IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new()
	{
		var query = DbSession.Query<T, TIndexCreator>();

		var lastArgType = typeof(TIndexCreator).BaseType?.GenericTypeArguments.LastOrDefault();

		if (lastArgType == null || IsNotTenantedType(lastArgType))
			return query;

		// Evaluate tenant separately from the WHERE condition, otherwise LINQ-to-JavaScript conversion fails
		var tenantId = _getCurrentTenantIdFunc();

		// Add an extra WHERE condition on the current tenant
		return query.Where(e => (e as ITenantedEntity)!.TenantId == tenantId);
	}
	#endregion Query [PUBLIC] ---------------------------------------------
		
	/// <inheritdoc />
	public bool HasChanges() =>
		// Usages of the direct property avoids opening a new session too early 
		_dbSession?.Advanced.HasChanges == true
		// a deferred patch query might exist
		|| _deferredPatchQueries.Any()
		// Note, the below has been added to 'Advanced.HasChanges()' in v5.2
		|| _dbSession?.Advanced is InMemoryDocumentSessionOperations { DeferredCommandsCount: > 0 };

	/// <inheritdoc />
	public string GetFullId<T>(string shortId) where T : IEntity
		=> DbSession.Advanced.GetFullId<T>(shortId);
		
	#region 'Patch' & 'Exists' for individual records [PUBLIC, PRIVATE] ---   
		
	/// <inheritdoc />
	public void PatchWithoutValidation<TEntity, TItem>(
			string shortId,
			Expression<Func<TEntity, IEnumerable<TItem>>> path,
			Expression<Func<JavaScriptArray<TItem>, object>> arrayAdder
		) where TEntity : IEntity
	{
		// Consider adding tenant validation in here
		DbSession.Advanced.Patch(GetFullId<TEntity>(shortId), path, arrayAdder);
	}

	/// <inheritdoc />
	public async Task<bool> Patch<TEntity, TProp>(string shortId, Expression<Func<TEntity, TProp>> path, TProp value) where TEntity : IEntity
	{
		var fullId = GetFullId<TEntity>(shortId);
		if (!IsNotTenantedType<TEntity>() && !await TenantedEntityExistsAsync<TEntity>(fullId))
		{
			ThrowArgumentExceptionIfRequired();
			return false;
		}
			
		DbSession.Advanced.Patch(fullId, path, value);
		return true;
	}

	/// <inheritdoc />
	public Task<bool> ExistsAsync<TEntity>(string shortId, CancellationToken token = default) where TEntity : IEntity
	{
		var fullId = GetFullId<TEntity>(shortId);
		return IsNotTenantedType<TEntity>() 
			? DbSession.Advanced.ExistsAsync(fullId, token) 
			: TenantedEntityExistsAsync<TEntity>(fullId, token);
	}

	/// <summary>
	///		Validate if a tenanted record exists
	/// </summary>
	private async Task<bool> TenantedEntityExistsAsync<TEntity>(string fullId, CancellationToken token = default) where TEntity : IEntity
	{
		var recordsTenantId = 
			await ( from e in Query<TEntity>() 
				where e.Id == fullId	// a condition to filter on tenant gets added in Query<T>() 
				select e.Id).SingleOrDefaultAsync(token);
		return recordsTenantId != null;
	}
	#endregion / 'Patch' & 'Exists' for individual records [PUBLIC, PRIVATE]
		
	#region Processing Deferred Path Queries [PUBLIC, PRIVATE] ------------

	/// <inheritdoc/>
	public IndexQuery GetIndexQuery<T>(IRavenQueryable<T> queryable)
	{
		var inspector = (IRavenQueryInspector)queryable;
		return inspector.GetIndexQuery(isAsync: true);
	}
	
	/// <inheritdoc/>
	public void AddDeferredPatchQuery(IndexQuery patchQuery)
	{
		// Consider adding tenant validation in here
		// E.g. by running Regex.Match(patchQuery.Query, @$"\b{nameof(ITenantedEntity.TenantId)}\b", RegexOptions.IgnoreCase);

		_deferredPatchQueries.Enqueue(patchQuery);
	}
		
	/// <summary>
	///		Collection of deferred patch queries.
	///		Will be executed after saving data in the main DB session
	/// </summary>
	private readonly ConcurrentQueue<IndexQuery> _deferredPatchQueries = new();
		
	/// <summary>
	///		Execute deferred queries.
	///		It gets called after saving data.
	/// </summary>
	private async Task SendDeferredPatchByQueryOperationsAsync(CancellationToken token)
	{
		if (token.IsCancellationRequested)
			return;
			
		while (_deferredPatchQueries.TryDequeue(out var queryIndex))
		{
			// The default timeout is not documented (seems to be around 15 sec)
			// Wait as long as it's required. It's better to fail (e.g. timeout on API response) rather than update on stale indexes 
			queryIndex.WaitForNonStaleResultsTimeout = TimeSpan.MaxValue;
			queryIndex.WaitForNonStaleResults = true;
			var sentOperation = await DbSession.Advanced.DocumentStore.Operations.SendAsync(new PatchByQueryOperation(queryIndex), DbSession.Advanced.SessionInfo, token);

			if (_waitingForIndexesAfterSave.HasValue)
				// Waiting comes in handy in the tests
				await sentOperation.WaitForCompletionAsync(_waitingForIndexesAfterSave.Value);
				
			if (token.IsCancellationRequested)
				return;
		}
	}
	#endregion / Processing Deferred Path Queries [PUBLIC, PRIVATE] -------

	#region Auxiliary methods [PRIVATE] -----------------------------------

	private IAsyncDocumentSession OpenAsyncDocumentSession()
	{
		var session = _sessionOptions != null 
			? _documentStore.OpenAsyncSession(_sessionOptions) 
			: _documentStore.OpenAsyncSession();
			
		if (_waitingForIndexesAfterSave.HasValue)
			// Wait on each change to avoid adding WaitForIndexing() in each case
			session.Advanced.WaitForIndexesAfterSaveChanges(_waitingForIndexesAfterSave.Value,false);

		return session;
	}

	private static bool IsNotTenantedType<T>() => IsNotTenantedType(typeof(T));
	private static bool IsNotTenantedType(Type type) => !type.GetInterfaces().Contains(typeof(ITenantedEntity));
		
	private bool HasCorrectTenant<T>(T entity) => (entity as ITenantedEntity)?.TenantId == _getCurrentTenantIdFunc();

	private void SetTenantIdOnEntity<T>(T entity) where T: notnull
	{
		if (IsNotTenantedType<T>()) return;
			
		var property = typeof(T).GetProperty(nameof(ITenantedEntity.TenantId));
		if (property == null)
			throw new ArgumentException("Can't resolve tenanted property");
			
		property.SetValue(entity, _getCurrentTenantIdFunc());
	}

	private void ThrowArgumentExceptionIfRequired()
	{
		if (ThrowExceptionOnWrongTenant)
			throw new ArgumentException("Attempt to access a record of another tenant");
	}
	#endregion / Auxiliary methods [PRIVATE] ------------------------------
}