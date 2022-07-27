using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.TicketImporter.Services;

internal interface ICustomFieldService
{
	/// <summary>
	///		Generate or fetch the Custom Field for preserving the reference to the original ticket
	/// </summary>
	/// <returns> The ID of the custom field </returns>
	Task<string> GenerateOrFetchUrlCustomField();
	/// <summary>
	///		Get all values for the custom field 
	/// </summary>
	Task<string[]> GetGitHubUrlsForExistingTickets(string customFieldId);
}

internal class CustomFieldService : ICustomFieldService
{
	private readonly ICustomFieldListQueryService _customFieldQueryService;
	private readonly ICustomFieldCommandService _customFieldCmdService;
	private readonly IAsyncTenantedDocumentSession _dbSession;

	private const string UrlCustomFieldName = "Original URL";

	public CustomFieldService(ICustomFieldCommandService customFieldCmdService,
	                          ICustomFieldListQueryService customFieldQueryService,
	                          IAsyncTenantedDocumentSession dbSession)
	{
		_customFieldCmdService = customFieldCmdService;
		_customFieldQueryService = customFieldQueryService;
		_dbSession = dbSession;
	}
		
	/// <inheritdoc/>
	public async Task<string> GenerateOrFetchUrlCustomField()
	{
		// Return the ID of the existing field (if exists)
		var existingFields = await _customFieldQueryService.GetArray(new CustomFieldListGetRequest { PageSize = int.MaxValue });
		var field = existingFields.FirstOrDefault(f => f.FieldType == CustomFieldType.Url && f.Name == UrlCustomFieldName);
		if (field is not null)
			return field.Id!;
			
		// Create a new field
		var (fieldRef, _) = await _customFieldCmdService.Create(new CustomFieldAddRequest { FieldType = CustomFieldType.Url, Name = UrlCustomFieldName, IsMandatory = false });
		await _dbSession.SaveChangesAsync();
		return fieldRef.Id;
	}

	/// <inheritdoc/>
	public async Task<string[]> GetGitHubUrlsForExistingTickets(string customFieldId)
	{
		var cf = await (
			from t in _dbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>()
			where t.CustomFields![customFieldId] != null
			select t.CustomFields!
		).ToArrayAsync();
		return cf.Where(c => c != null).Select(c => c![customFieldId].ToString()!).ToArray();
	}
}