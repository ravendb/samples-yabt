using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands.DTOs;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.WebApi.Controllers
{
	[ApiController]
	[Route("api/backlogItems/{backlogItemId}/comments")]
	public class BacklogItemCommentsController : ControllerBase
	{
		/// <summary>
		///		Get comments for a Backlog Item
		/// </summary>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ListResponse<BacklogItemCommentListGetResponse>> GetComments([FromServices] IBacklogItemByIdQueryService service,
		                                                                         [FromRoute] string backlogItemId,
		                                                                         [FromQuery] BacklogItemCommentListGetRequest @params)
			=> service.GetBacklogItemComments(backlogItemId, @params);

		/// <summary>
		///		Create a new comment
		/// </summary>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemCommentReference>> Create([FromServices] IBacklogItemCommentCommandService service, 
		                                                              [FromRoute] string backlogItemId,
		                                                              [FromQuery] CommentAddUpdRequest dto) 
			=> service.Create(backlogItemId, dto).ToActionResultOfT();

		/// <summary>
		///		Update a comment
		/// </summary>
		[HttpPut("{commentId}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemCommentReference>> Update([FromServices] IBacklogItemCommentCommandService service,
		                                                              [FromRoute] string backlogItemId,
		                                                              [FromRoute] string commentId,
		                                                              CommentAddUpdRequest dto)
			=> service.Update(backlogItemId, commentId, dto).ToActionResultOfT();
		
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
