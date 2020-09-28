using System.ComponentModel.DataAnnotations;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs
{
	public class CommentDelRequest
	{
		[Required]
		public string TicketId { get; set; } = null!;
		[Required]
		public string CommentId { get; set; } = null!;
	}
}
