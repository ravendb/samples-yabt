using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BacklogItemsController : ControllerBase
{
	#region GET requests --------------------

	/// <summary>
	///		Get a paged list of Backlog Items
	/// </summary>
	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public Task<ListResponse<BacklogItemListGetResponse>> GetList([FromServices] IBacklogItemListQueryService service,
	                                                              [FromQuery] BacklogItemListGetRequest dto)
		=> service.GetList(dto);

	/// <summary>
	///		Get a single Backlog Item by ID with the 1st page of comments
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemGetResponseBase>> GetById([FromServices] IBacklogItemByIdQueryService service,
	                                                              [FromRoute] string id)
		=> service.GetById(id).ToActionResultOfT();

	/// <summary>
	///		Get a list of Backlog Items tags
	/// </summary>
	[HttpGet("tags")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public Task<BacklogItemTagListGetResponse[]> GetTags([FromServices] IBacklogItemListQueryService service,
	                                                     [FromQuery] BacklogItemTagListGetRequest dto)
		=> service.GetTags(dto);

	#endregion / GET requests ---------------

	#region POST requests -------------------

	/// <summary>
	///		Create a new bug
	/// </summary>
	/// <remarks> 
	///		.NET is not friendly with ambiguous input parameters in controller methods. It has no support of generic controller methods.
	///		It could be possible to have one POST (create) method and use 'dynamic' type of <paramref name="dto"/>, but finding all derived classes from <see cref="BacklogItemAddUpdRequestBase"/>
	///		and matching them to our DTO would be on our shoulders.
	/// </remarks>
	[HttpPost("bug")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public Task<ActionResult<BacklogItemReference>> Create([FromServices] IBacklogItemCommandService service, 
	                                                       BugAddUpdRequest dto) 
		=> service.Create(dto).ToActionResultOfT();

	/// <summary>
	///		Create a new user story
	/// </summary>
	[HttpPost("story")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public Task<ActionResult<BacklogItemReference>> Create([FromServices] IBacklogItemCommandService service, 
	                                                       UserStoryAddUpdRequest dto) 
		=> service.Create(dto).ToActionResultOfT();

	/// <summary>
	///		Create a new task
	/// </summary>
	[HttpPost("task")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public Task<ActionResult<BacklogItemReference>> Create([FromServices] IBacklogItemCommandService service, 
	                                                       TaskAddUpdRequest dto) 
		=> service.Create(dto).ToActionResultOfT();

	/// <summary>
	///		Create a new feature
	/// </summary>
	[HttpPost("feature")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public Task<ActionResult<BacklogItemReference>> Create([FromServices] IBacklogItemCommandService service, 
	                                                       FeatureAddUpdRequest dto) 
		=> service.Create(dto).ToActionResultOfT();

	#endregion / POST requests --------------

	#region PUT requests --------------------

	/// <summary>
	///		Update a bug
	/// </summary>
	/// <remarks> 
	///		The same as for the POST methods.
	///		.NET is not friendly with ambiguous input parameters in controller methods. It has no support of generic controller methods.
	///		It could be possible to have one POST (create) method and use 'dynamic' type of <paramref name="dto"/>, but finding all derived classes from <see cref="BacklogItemAddUpdRequestBase"/>
	///		and matching them to our DTO would be on our shoulders.
	/// </remarks>
	[HttpPut("{id}/bug")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemReference>> Update([FromServices] IBacklogItemCommandService service,
	                                                       [FromRoute] string id,
	                                                       BugAddUpdRequest dto)
		=> service.Update(id, dto).ToActionResultOfT();

	/// <summary>
	///		Update a user story
	/// </summary>
	[HttpPut("{id}/story")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemReference>> Update([FromServices] IBacklogItemCommandService service,
	                                                       [FromRoute] string id,
	                                                       UserStoryAddUpdRequest dto)
		=> service.Update(id, dto).ToActionResultOfT();

	/// <summary>
	///		Update a task
	/// </summary>
	[HttpPut("{id}/task")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemReference>> Update([FromServices] IBacklogItemCommandService service,
	                                                       [FromRoute] string id,
	                                                       TaskAddUpdRequest dto)
		=> service.Update(id, dto).ToActionResultOfT();

	/// <summary>
	///		Update a feature
	/// </summary>
	[HttpPut("{id}/feature")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemReference>> Update([FromServices] IBacklogItemCommandService service,
	                                                       [FromRoute] string id,
	                                                       FeatureAddUpdRequest dto)
		=> service.Update(id, dto).ToActionResultOfT();

	/// <summary>
	///		Set the assignee of a Backlog Item
	/// </summary>
	[HttpPut("{id}/assignee")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<IActionResult> AssignToUser([FromServices] IBacklogItemCommandService service, [FromRoute] string id, string userId) 
		=> service.AssignToUser(id, userId).ToActionResult();

	/// <summary>
	///		Set a new state for a Backlog Item
	/// </summary>
	[HttpPut("{id}/state")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<IActionResult> SetState([FromServices] IBacklogItemCommandService service, [FromRoute] string id, BacklogItemState newState) 
		=> service.SetState(id, newState).ToActionResult();

	#endregion / PUT requests ---------------

	/// <summary>
	///		Delete a Backlog Item
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<BacklogItemReference>> Delete([FromServices] IBacklogItemCommandService service,
	                                                       [FromRoute] string id)
		=> service.Delete(id).ToActionResultOfT();
}