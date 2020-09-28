using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands
{
	public interface IBacklogItemCommandService
	{
		Task<BacklogItemReference> Create<T>(T dto) where T : BacklogItemAddUpdRequest;

		Task<BacklogItemReference?> Update<T>(string id, T dto) where T : BacklogItemAddUpdRequest;

		Task<BacklogItemReference?> Delete(string id);

		Task<BacklogItemReference?> AssignToUser(string backlogItemId, string userId);
	}
}
