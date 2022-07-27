using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ByCustomFieldQuery;

public interface IBacklogItemByCustomFieldQueryService
{
	Task<int> GetCountOfBacklogItemsUsingCustomField(string customFieldId);
}
	
public class BacklogItemByCustomFieldQueryService : BaseService<BacklogItem>, IBacklogItemByCustomFieldQueryService
{
	public BacklogItemByCustomFieldQueryService(IAsyncTenantedDocumentSession dbSession) : base(dbSession) { }

	public Task<int> GetCountOfBacklogItemsUsingCustomField(string customFieldId)
	{
		var query = 
			DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
			         // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
			         .Where(i => i.CustomFields![customFieldId] != null);
		return query.CountAsync();
	}
}