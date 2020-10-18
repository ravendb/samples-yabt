using System.ComponentModel.DataAnnotations;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs
{
	public class CommentAddRequest
	{
		[Required]
		public string Message { get; set; } = null!;
	}
}
