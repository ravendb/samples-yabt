using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands;

public interface IBacklogItemCommandService
{
	Task<IDomainResult<BacklogItemReference>> Create<T>(T dto) where T : BacklogItemAddUpdRequestBase;

	Task<IDomainResult<BacklogItemReference>> Update<T>(string id, T dto) where T : BacklogItemAddUpdRequestBase;

	Task<IDomainResult<BacklogItemReference>> Delete(string id);

	Task<IDomainResult> SetState(string backlogItemId, BacklogItemState newState);
		
	Task<IDomainResult> AssignToUser(string backlogItemId, string? userShortenId);
}