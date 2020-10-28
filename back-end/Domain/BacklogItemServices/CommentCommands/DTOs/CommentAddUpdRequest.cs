using System.ComponentModel.DataAnnotations;

namespace Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs
{
	public class CommentAddUpdRequest
	{
		[Required]
		public string Message { get; set; } = null!;
	}
}
