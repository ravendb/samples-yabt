using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.Common
{
	public abstract class BaseService<TEntity> where TEntity : IEntity
	{
		protected readonly IAsyncDocumentSession DbSession;

		protected BaseService(IAsyncDocumentSession dbSession)
		{
			DbSession = dbSession;
		}

		protected string GetFullId(string id) => $"{typeof(TEntity).Name}s/{id}";
	}
}
