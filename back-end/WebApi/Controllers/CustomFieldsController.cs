using System.Threading.Tasks;

using DomainResults.Mvc;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomFieldsController : ControllerBase
{
	/// <summary>
	///		Get a list of 'Custom Fields'
	/// </summary>
	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public Task<ListResponse<CustomFieldListGetResponse>> GetList([FromServices] ICustomFieldListQueryService service,
	                                                              [FromQuery] CustomFieldListGetRequest dto)
		=> service.GetList(dto);

	/// <summary>
	///		Get a single 'Custom Field' by ID
	/// </summary>
	[HttpGet("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<CustomFieldItemResponse>> GetById([FromServices] ICustomFieldByIdQueryService service,
	                                                           [FromRoute] string id)
		=> service.GetById(id).ToActionResultOfT();

	/// <summary>
	///		Create a new 'Custom Fields'
	/// </summary>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public Task<ActionResult<CustomFieldReferenceDto>> Create(	[FromServices] ICustomFieldCommandService service, 
	                                                            CustomFieldAddRequest dto) 
		=> service.Create(dto).ToActionResultOfT();

	/// <summary>
	///		Update a 'Custom Fields'
	/// </summary>
	[HttpPut("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<CustomFieldReferenceDto>> Update(	[FromServices] ICustomFieldCommandService service,
	                                                            [FromRoute] string id,
	                                                            CustomFieldUpdateRequest dto)
		=> service.Update(id, dto).ToActionResultOfT();

	/// <summary>
	///		Delete a 'Custom Fields'
	/// </summary>
	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public Task<ActionResult<CustomFieldReferenceDto>> Delete(	[FromServices] ICustomFieldCommandService service,
	                                                            [FromRoute] string id)
		=> service.Delete(id).ToActionResultOfT();
}