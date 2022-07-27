using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Domain.Common;

public abstract class BaseService<TEntity>: BaseDbService where TEntity : IEntity
{
	protected BaseService(IAsyncTenantedDocumentSession dbSession): base(dbSession) {}

	protected string GetFullId(string id) 
		=> id.Contains('/') 
			? id	// Assume it's already a full ID with a prefix  
			: DbSession.GetFullId<TEntity>(id);
}

public abstract class BaseDbService
{
	protected readonly IAsyncTenantedDocumentSession DbSession;

	protected BaseDbService(IAsyncTenantedDocumentSession dbSession)
	{
		DbSession = dbSession;
	}
}