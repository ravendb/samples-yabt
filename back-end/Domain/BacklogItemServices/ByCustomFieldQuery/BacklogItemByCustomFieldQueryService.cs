using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.BacklogItemServices.ByCustomFieldQuery
{
	public interface IBacklogItemByCustomFieldQueryService
	{
		Task<int> GetCountOfBacklogItemsUsingCustomField(string customFieldId);
	}
	
	public class BacklogItemByCustomFieldQueryService : BaseService<BacklogItem>, IBacklogItemByCustomFieldQueryService
	{
		public BacklogItemByCustomFieldQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		public Task<int> GetCountOfBacklogItemsUsingCustomField(string customFieldId)
		{
			var customFieldIdForIndex = DbSession.GetIdForDynamicField<CustomField>(customFieldId);
			var query = DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
			                     .Where(i => i.CustomFields![customFieldIdForIndex] != null);
			return query.CountAsync();
		}
	}
}
