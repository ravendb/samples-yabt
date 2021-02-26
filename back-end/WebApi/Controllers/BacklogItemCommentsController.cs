using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;

namespace Raven.Yabt.WebApi.Controllers
{
	[ApiController]
	[Route("api/backlogItems/{backlogItemId}/comments")]
	public class BacklogItemCommentsController : ControllerBase
	{
		/// <summary>
		///		Create a new comment
		/// </summary>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemCommentReference>> Create([FromServices] IBacklogItemCommentCommandService service, 
		                                                              [FromRoute] string backlogItemId,
		                                                              [FromQuery] string message) 
			=> service.Create(backlogItemId, message).ToActionResultOfT();

		/// <summary>
		///		Update a comment
		/// </summary>
		[HttpPut("{commentId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemCommentReference>> Update([FromServices] IBacklogItemCommentCommandService service,
		                                                              [FromRoute] string backlogItemId,
		                                                              [FromRoute] string commentId,
		                                                              string message)
			=> service.Update(backlogItemId, commentId, message).ToActionResultOfT();
		
		/// <summary>
		///		Delete a comment
		/// </summary>
		[HttpDelete("{commentId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemCommentReference>> Delete([FromServices] IBacklogItemCommentCommandService service,
		                                                              [FromRoute] string backlogItemId,
		                                                              [FromRoute] string commentId)
			=> service.Delete(backlogItemId, commentId).ToActionResultOfT();
	}
}
