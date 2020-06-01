using System.Threading.Tasks;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Models.BacklogItem;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery
{
	public class BacklogItemByIdQueryService : BaseService<BacklogItem>, IBacklogItemByIdQueryService
	{
		public BacklogItemByIdQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		public async Task<BacklogItemGetResponse?> GetById(string id)
		{
			var fullId = GetFullId(id);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);

			return (ticket?.Type) switch
			{
				BacklogItemType.Bug			=> (ticket as BacklogItemBug)?.ConvertToDto<BacklogItemBug, BugGetResponse>(),
				BacklogItemType.UserStory	=> (ticket as BacklogItemUserStory)?.ConvertToDto<BacklogItemUserStory, UserStoryGetResponse>(),
				_ => null,
			};
		}
	}
}
