using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

public class CustomFieldItemResponse
{
	public string Name { get; set; } = null!;

	public CustomFieldType FieldType { get; set; }
		
	public bool IsMandatory { get; set; }
		
	/// <summary>
	///		Types of tickets the field is going to be used for (bugs, user stories or any type)
	/// </summary>
	public BacklogItemType?[]? BacklogItemTypes { get; set; }
		
	/// <summary>
	///		Number of backlog items the field is used
	/// </summary>
	public int UsedInBacklogItemsCount { get; set; }
}