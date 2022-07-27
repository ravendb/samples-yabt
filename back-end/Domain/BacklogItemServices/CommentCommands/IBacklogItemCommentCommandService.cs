using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands;

public interface IBacklogItemCommentCommandService
{
	Task<IDomainResult<BacklogItemCommentReference>> Create(string backlogItemId, string message);
	Task<IDomainResult<BacklogItemCommentReference>> Update(string backlogItemId, string commentId, string message);
	Task<IDomainResult<BacklogItemCommentReference>> Delete(string backlogItemId, string commentId);
}