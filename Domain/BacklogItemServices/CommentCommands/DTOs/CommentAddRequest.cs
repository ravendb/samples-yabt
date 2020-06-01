using System.ComponentModel.DataAnnotations;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs
{
	public class CommentAddRequest
	{
		[Required]
		public string TicketId { get; set; } = null!;
		[Required]
		public string UserId { get; set; } = null!;
		[Required]
		public string Message { get; set; } = null!;
	}
}
