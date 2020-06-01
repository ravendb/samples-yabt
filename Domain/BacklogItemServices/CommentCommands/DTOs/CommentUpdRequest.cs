using System.ComponentModel.DataAnnotations;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs
{
	public class CommentUpdRequest : CommentAddRequest
	{
		[Required]
		public string CommentId { get; set; } = null!;
	}
}
