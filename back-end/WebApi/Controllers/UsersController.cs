using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;
using Raven.Yabt.Domain.UserServices.Query;
using Raven.Yabt.Domain.UserServices.Query.DTOs;
using Raven.Yabt.WebApi.Controllers.DTOs;

namespace Raven.Yabt.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
	#region GET requests --------------------

	/// <summary>
	///		Get a single user by ID
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<UserGetByIdResponse>> GetById([FromServices] IUserQueryService service,
	                                                       [FromRoute] string id)
		=> service.GetById(id).ToActionResultOfT();

	/// <summary>
	///		Get the current user
	/// </summary>
	[HttpGet("current")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser([FromServices] IUserQueryService service,
	                                                                    [FromServices] ICurrentUserResolver userResolver)
	{
		var currentId = userResolver.GetCurrentUserId();
		var userResult = await service.GetById(currentId);
		return !userResult.IsSuccess 
			? userResult.To<CurrentUserResponse>().ToActionResultOfT() 
			: new CurrentUserResponse(userResult.Value, currentId);
	}

	/// <summary>
	///		Get a paged list of Users
	/// </summary>
	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public Task<ListResponse<UserListGetResponse>> GetList([FromServices] IUserQueryService service,
	                                                       [FromQuery] UserListGetRequest dto)
		=> service.GetList(dto);

	#endregion / GET requests ---------------

	#region POST / PUT / DELETE requests ----

	/// <summary>
	///		Create a new User
	/// </summary>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public Task<ActionResult<UserReference>> Create([FromServices] IUserCommandService service, UserAddUpdRequest dto) 
		=> service.Create(dto).ToActionResultOfT();

	/// <summary>
	///		Update a User
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<UserReference>> Update([FromServices] IUserCommandService service,
	                                                [FromRoute] string id,
	                                                UserAddUpdRequest dto)
		=> service.Update(id, dto).ToActionResultOfT();

	/// <summary>
	///		Delete a User
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<UserReference>> Delete([FromServices] IUserCommandService service,
	                                                [FromRoute] string id)
		=> service.Delete(id).ToActionResultOfT();

	#endregion / POST / PUT / DELETE requests
}