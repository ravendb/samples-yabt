using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CustomFieldsController : ControllerBase
	{
		/// <summary>
		///		Get a list of 'Custom Fields'
		/// </summary>
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public Task<CustomFieldListGetResponse[]> GetList([FromServices] ICustomFieldQueryService service,
														  [FromQuery] CustomFieldListGetRequest dto
														)
			=> service.GetArray(dto);

		/// <summary>
		///		Create a new 'Custom Fields'
		/// </summary>
		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public Task<ActionResult<CustomFieldReferenceDto>> Create(	[FromServices] ICustomFieldCommandService service, 
																	CustomFieldAddRequest dto
																) 
			=> service.Create(dto).ToActionResultOfT();

		/// <summary>
		///		Rename a 'Custom Fields'
		/// </summary>
		[HttpPut("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<CustomFieldReferenceDto>> Update(	[FromServices] ICustomFieldCommandService service,
																	[FromRoute] string id,
																	CustomFieldRenameRequest dto
																)
			=> service.Rename(id, dto).ToActionResultOfT();

		/// <summary>
		///		Delete a 'Custom Fields'
		/// </summary>
		[HttpDelete("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public Task<ActionResult<CustomFieldReferenceDto>> Delete(	[FromServices] ICustomFieldCommandService service,
																	[FromRoute] string id
																)
			=> service.Delete(id).ToActionResultOfT();
	}
}
