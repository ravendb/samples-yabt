using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands
{
	public interface IBacklogItemCommentCommandService
	{
		Task<IDomainResult<BacklogItemCommentReference>> Create(string backlogItemId, CommentAddRequest dto);
		Task<IDomainResult<BacklogItemCommentReference>> Update(string backlogItemId, CommentUpdRequest dto);
		Task<IDomainResult<BacklogItemCommentReference>> Delete(string backlogItemId, CommentDelRequest dto);
	}
}
