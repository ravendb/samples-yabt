using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands
{
	public interface IBacklogItemCommentCommandService
	{
		Task<IDomainResult<BacklogItemCommentReference>> Create(string backlogItemId, CommentAddUpdRequest dto);
		Task<IDomainResult<BacklogItemCommentReference>> Update(string backlogItemId, string commentId, CommentAddUpdRequest dto);
		Task<IDomainResult<BacklogItemCommentReference>> Delete(string backlogItemId, string commentId);
	}
}
