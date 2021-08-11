using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Queries;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Database.Infrastructure
{
	/// <summary>
	///		A multi-tenanted DB session, a wrapper on <see cref="IAsyncDocumentSession"/>.
	///		Automatically filters entities implementing <see cref="ITenantedEntity"/> by the current tenant ID.
	/// </summary>
	/// <remarks>
	///		All the <seealso cref="IAsyncDocumentSession"/> methods that won't support multi-tenancy checks (e.g. Delete(string)) have been omitted
	/// </remarks>
	public interface IAsyncTenantedDocumentSession : IDisposable
	{
		/// <summary>
		///		Direct access to <seealso cref="IAsyncDocumentSession.Advanced"/> property
		/// </summary>
		IAsyncAdvancedSessionOperations Advanced { get; }
		
		/// <summary>
		///		Flag defining the behaviour on requesting a record of a wrong tenant.
		///		<value>true</value> - would throw an exception. <value>false</value> - silent return of NULL.
		/// </summary>
		bool ThrowExceptionOnWrongTenant { get; }

		/// <summary>
		///		Check if there are any changes to save or run any patch requests
		///		An extension of the <see cref="IAdvancedDocumentSessionOperations.HasChanges"/> property usually accessed from <seealso cref="IAsyncDocumentSession.Advanced"/>
		/// </summary>
		bool HasChanges();
		
		/// <summary>
		/// 	Add a RavenDB patch request for executing after calling <see cref="IAsyncDocumentSession.SaveChangesAsync"/> 
		/// </summary>
		/// <remarks>
		///		Operation against indexes can't be performed in the same transaction. This method offloads the operation to the Server to run
		/// </remarks>
		void AddDeferredPatchQuery(IndexQuery patchQuery);

		/// <summary>
		///		Extension of the <seealso cref="IAsyncDocumentSession.CountersFor(object)"/> method to get counters for the entity.
		///		Automatically checks the current tenant if <paramref name="entity"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="entity"> Instance of entity </param>
		/// <exception cref="ArgumentException"> Throws an exception for incorrectly requested tenant if <see cref="ThrowExceptionOnWrongTenant"/> is set. </exception>
		IAsyncSessionDocumentCounters? CountersFor<T>(T entity) where T: notnull;
		
		/// <summary>
		///     Extension of the <seealso cref="IAsyncDocumentSession.Delete{T}"/> method to mark the specified entity for deletion.
		///		Automatically checks the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="entity"> Instance of entity to delete</param>
		/// <exception cref="ArgumentException"> Throws an exception for <paramref name="entity"/> with wrong tenant if <see cref="ThrowExceptionOnWrongTenant"/> is set. </exception>
		void Delete<T>(T entity) where T: notnull;

		/// <summary>
		///		Saves all the pending changes to the server.
		///		Propagation of the <seealso cref="IAsyncDocumentSession.SaveChangesAsync"/>
		/// </summary>
		Task SaveChangesAsync(CancellationToken token = default);

		/// <summary>
		///     Extension of the <seealso cref="IAsyncDocumentSession.StoreAsync(object, CancellationToken)"/> method to stores entity in session.
		///		Automatically adds the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="entity"> The entity to store. </param>
		/// <param name="token"> The cancellation token. </param>
		Task StoreAsync<T>(T entity, CancellationToken token = default) where T: notnull;
		/// <summary>
		///     Extension of the <seealso cref="IAsyncDocumentSession.StoreAsync(object, string, CancellationToken)"/> method to stores entity in session.
		///		Automatically adds the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="entity"> The entity to store. </param>
		/// <param name="id"> Id to store this entity under. If other entity exists with the same id it will be overwritten.</param>
		/// <param name="token"> The cancellation token. </param>
		Task StoreAsync<T>(T entity, string id, CancellationToken token = default) where T: notnull;
		/// <summary>
		///     Extension of the <seealso cref="IAsyncDocumentSession.StoreAsync(object, string, string, CancellationToken)"/> method to stores entity in session.
		///		Automatically adds the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="entity"> The entity to store. </param>
		/// <param name="changeVector">  </param>
		/// <param name="id"> Id to store this entity under. If other entity exists with the same id it will be overwritten.</param>
		/// <param name="token"> The cancellation token. </param>
		Task StoreAsync<T>(T entity, string changeVector, string id, CancellationToken token = default) where T: notnull;

		/// <summary>
		///		Extension of the <seealso cref="IAsyncDocumentSession.LoadAsync{T}(string, CancellationToken)"/> method to load an entity with the specified id.
		///		Automatically checks the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="id">Identifier of the entity that will be loaded.</param>
		/// <param name="token">The cancellation token.</param>
		/// <exception cref="ArgumentException"> Throws an exception for entity with wrong tenant if <see cref="ThrowExceptionOnWrongTenant"/> is set. </exception>
		Task<T?> LoadAsync<T>(string id, CancellationToken token = default);
		/// <summary>
		///		Extension of the <seealso cref="IAsyncDocumentSession.LoadAsync{T}(IEnumerable{string}, CancellationToken)"/> method to load entities with the multiple specified ids.
		///		Automatically checks the current tenant for all loaded records if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <param name="ids">Identifier of the entities that will be loaded.</param>
		/// <param name="token">The cancellation token.</param>
		/// <exception cref="ArgumentException"> Throws an exception when entities with wrong tenant found and if <see cref="ThrowExceptionOnWrongTenant"/> is set. </exception>
		Task<Dictionary<string, T>> LoadAsync<T>(IEnumerable<string> ids, CancellationToken token = default);

		/// <summary>
		///     Extension of the <seealso cref="IAsyncDocumentSession.Query{T}"/> method to query the index specified by <typeparamref name="TIndexCreator" />.
		///		Automatically adds a filter on the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <typeparam name="T">The result of the query</typeparam>
		/// <typeparam name="TIndexCreator">The type of the index creator.</typeparam>
		IRavenQueryable<T> Query<T, TIndexCreator>() where TIndexCreator : AbstractIndexCreationTask, new();
		/// <summary>
		///		Extension of the <seealso cref="IAsyncDocumentSession.Query{T}"/> method to query the specified index.
		///		Automatically adds a filter on the current tenant if <typeparamref name="T"/> implements <see cref="ITenantedEntity"/>.
		/// </summary>
		/// <typeparam name="T"> The result of the query </typeparam>
		/// <param name="indexName"> Name of the index (mutually exclusive with collectionName) </param>
		/// <param name="collectionName"> Name of the collection (mutually exclusive with indexName) </param>
		/// <param name="isMapReduce"> Whether we are querying a map/reduce index (modify how we treat identifier properties) </param>
		IRavenQueryable<T> Query<T>(string? indexName = null, string? collectionName = null, bool isMapReduce = false);
	}
}
