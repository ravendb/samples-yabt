using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.Common
{
	public abstract class BaseService<TEntity> where TEntity : IEntity
	{
		protected readonly IAsyncDocumentSession DbSession;

		protected BaseService(IAsyncDocumentSession dbSession)
		{
			DbSession = dbSession;
		}

		protected string GetFullId(string id) => DbSession.GetFullId<TEntity>(id);
	}
}
