using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

public class CustomFieldListGetResponse: ListResponseWithSanitisedIds
{
	public string Name { get; set; } = null!;

	public CustomFieldType FieldType { get; set; }
		
	public bool IsMandatory { get; set; }
		
	/// <summary>
	///		Types of tickets the field is going to be used for (bugs, user stories or any type)
	/// </summary>
	public BacklogItemType?[]? BacklogItemTypes { get; set; }
}