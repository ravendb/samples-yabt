using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.CommentCommands;
using Raven.Yabt.WebApi.Controllers.DTOs;

namespace Raven.Yabt.WebApi.Controllers;

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
	                                                              BacklogItemCommentAddUpdateRequest @params) 
		=> service.Create(backlogItemId, @params.Message).ToActionResultOfT();

	/// <summary>
	///		Update a comment
	/// </summary>
	[HttpPut("{commentId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemCommentReference>> Update([FromServices] IBacklogItemCommentCommandService service,
	                                                              [FromRoute] string backlogItemId,
	                                                              [FromRoute] string commentId,
	                                                              BacklogItemCommentAddUpdateRequest @params)
		=> service.Update(backlogItemId, commentId, @params.Message).ToActionResultOfT();
		
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