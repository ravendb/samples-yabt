using System.Collections.Generic;
using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BacklogItemsController : ControllerBase
	{
		#region GET requests --------------------

		/// <summary>
		///		Get a single Backlog Item by ID
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemGetResponse>> GetById (	[FromServices] IBacklogItemByIdQueryService service,
																	[FromRoute] string id
																  )
			=> service.GetById(id).ToActionResultOfT();

		/// <summary>
		///		Get a paged list of Backlog Items
		/// </summary>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public Task<List<BacklogItemListGetResponse>> GetList([FromServices] IBacklogItemListQueryService service,
															  [FromQuery] BacklogItemListGetRequest dto
															)
			=> service.GetList(dto);

		#endregion / GET requests ---------------

		#region POST requests -------------------

		/// <summary>
		///		Create a new bug
		/// </summary>
		/// <remarks> 
		///		.NET is not friendly with ambigous input parameters in controller methods. It has no support of generic controller methods.
		///		It could be possible to have one POST (create) method and use 'dynamic' type of <paramref name="dto"/>, but finding all derived classes from <see cref="BacklogItemAddUpdRequest"/>
		///		and matching them to our DTO would be on our shoulders.
		/// </remarks>
		[HttpPost("bug")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<ActionResult<BacklogItemReference>> Create([FromServices] IBacklogItemCommandService service, BugAddUpdRequest dto	)		=> service.Create(dto).ToActionResultOfT();

		/// <summary>
		///		Create a new user story
		/// </summary>
		/// <remarks> 
		///		.NET is not friendly with ambigous input parameters in controller methods. It has no support of generic controller methods.
		///		It could be possible to have one POST (create) method and use 'dynamic' type of <paramref name="dto"/>, but finding all derived classes from <see cref="BacklogItemAddUpdRequest"/>
		///		and matching them to our DTO would be on our shoulders.
		/// </remarks>
		[HttpPost("story")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<ActionResult<BacklogItemReference>> Create([FromServices] IBacklogItemCommandService service, UserStoryAddUpdRequest dto)	=> service.Create(dto).ToActionResultOfT();

		#endregion / POST requests --------------

		#region PUT requests --------------------

		/// <summary>
		///		Update a bug
		/// </summary>
		/// <remarks> 
		///		The same as for the POST methods.
		///		.NET is not friendly with ambigous input parameters in controller methods. It has no support of generic controller methods.
		///		It could be possible to have one POST (create) method and use 'dynamic' type of <paramref name="dto"/>, but finding all derived classes from <see cref="BacklogItemAddUpdRequest"/>
		///		and matching them to our DTO would be on our shoulders.
		/// </remarks>
		[HttpPut("{id}/bug")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemReference>> Update( [FromServices] IBacklogItemCommandService service,
																[FromRoute] string id,
																BugAddUpdRequest dto
																)
			=> service.Update(id, dto).ToActionResultOfT();

		/// <summary>
		///		Update a user story
		/// </summary>
		/// <remarks> 
		///		The same as for the POST methods.
		///		.NET is not friendly with ambigous input parameters in controller methods. It has no support of generic controller methods.
		///		It could be possible to have one POST (create) method and use 'dynamic' type of <paramref name="dto"/>, but finding all derived classes from <see cref="BacklogItemAddUpdRequest"/>
		///		and matching them to our DTO would be on our shoulders.
		/// </remarks>
		[HttpPut("{id}/story")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemReference>> Update( [FromServices] IBacklogItemCommandService service,
																[FromRoute] string id,
																UserStoryAddUpdRequest dto
																)
			=> service.Update(id, dto).ToActionResultOfT();

		/// <summary>
		///		Set the assignee of a Backlog Item
		/// </summary>
		[HttpPut("{id}/assignee")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemReference>> AssignToUser([FromServices] IBacklogItemCommandService service,
																	 [FromRoute] string id,
																	 string userId
																	)
			=> service.AssignToUser(id, userId).ToActionResultOfT();

		#endregion / PUT requests ---------------

		/// <summary>
		///		Delete a Backlog Item
		/// </summary>
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<BacklogItemReference>> Delete ([FromServices] IBacklogItemCommandService service,
																[FromRoute] string id
																)
			=> service.Delete(id).ToActionResultOfT();
	}
}
