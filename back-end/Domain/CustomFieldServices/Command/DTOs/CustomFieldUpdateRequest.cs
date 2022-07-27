using System.ComponentModel.DataAnnotations;

using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

public class CustomFieldUpdateRequest
{
	[Required] 
	public string Name { get; set; } = null!;
		
	public bool? IsMandatory { get; set; }	
		
	/// <summary>
	///		Types of tickets the field is going to be used for (bugs, user stories or any type)
	/// </summary>
	public BacklogItemType?[]? BacklogItemTypes { get; set; }
}