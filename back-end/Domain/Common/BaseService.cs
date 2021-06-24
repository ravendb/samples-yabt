using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Domain.Common
{
	public abstract class BaseService<TEntity> where TEntity : IEntity
	{
		protected readonly IAsyncTenantedDocumentSession DbSession;

		protected BaseService(IAsyncTenantedDocumentSession dbSession)
		{
			DbSession = dbSession;
		}

		protected string GetFullId(string id) 
			=> id.Contains('/') 
				? id	// Assume it's already a full ID with a prefix  
				: DbSession.GetFullId<TEntity>(id);
	}
}
