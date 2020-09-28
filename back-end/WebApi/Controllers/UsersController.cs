using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Raven.Yabt.Domain.UserServices;
using Raven.Yabt.Domain.UserServices.DTOs;

namespace WebApi.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly ILogger<UsersController> _logger;

		public UsersController(ILogger<UsersController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		///		Get a single user by ID
		/// </summary>
		[HttpGet("{id}")]
		public async Task<ActionResult<UserGetByIdResponse>> Get([FromServices] IUserQueryService service, [FromRoute] string id)
		{
			return await service.GetById(id);
		}
	}
}
