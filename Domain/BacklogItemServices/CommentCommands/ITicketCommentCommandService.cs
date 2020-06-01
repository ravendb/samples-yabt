using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands
{
	public interface ITicketCommentCommandService
	{
		Task<IEntityReference> AddComment(CommentAddRequest dto);
		Task<IEntityReference> EditComment(CommentUpdRequest dto);
		Task<IEntityReference> DeleteComment(CommentDelRequest dto);
	}
}
