using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Domain.UserServices;
using Raven.Yabt.Domain.UserServices.DTOs;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		/// <summary>
		///		Get a single user by ID
		/// </summary>
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<UserGetByIdResponse>> Get ([FromServices] IUserQueryService service, 
															[FromRoute] string id
														   ) 
			=> service.GetById(id).ToActionResultOfT();
	}
}
