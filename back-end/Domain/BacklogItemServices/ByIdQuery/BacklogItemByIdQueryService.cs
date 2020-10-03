using System;
using System.Threading.Tasks;

using DomainResults.Common;

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

		public async Task<IDomainResult<BacklogItemGetResponse>> GetById(string id)
		{
			var fullId = GetFullId(id);

			var ticket = await DbSession.LoadAsync<BacklogItem>(fullId);
			if (ticket == null)
				return DomainResult.NotFound<BacklogItemGetResponse>();

			var dto = (ticket.Type) switch
			{
				BacklogItemType.Bug			=> (ticket as BacklogItemBug)		?.ConvertToDto<BacklogItemBug, BugGetResponse>() as  BacklogItemGetResponse,
				BacklogItemType.UserStory	=> (ticket as BacklogItemUserStory)	?.ConvertToDto<BacklogItemUserStory, UserStoryGetResponse>() as BacklogItemGetResponse,
				_ => throw new NotImplementedException($"Not supported Backlog Item Type: {ticket.Type}"),
			};
			if (dto == null)
				throw new NotSupportedException($"Failed to return Backlog Item type of {ticket.Type}");

			return DomainResult.Success(dto);
		}
	}
}
