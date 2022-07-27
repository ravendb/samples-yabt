using System.Collections.Generic;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;

public class BacklogItemListGetRequest : ListRequest<BacklogItemsOrderColumns>
{
	/// <summary>
	///		Request items with specified types (user story, task, etc.)		
	/// </summary>
	/// <remarks>
	///		The (enum | null)[] type helps to prevent  "The value '' is invalid." validation error.
	///		See the problem description at https://stackoverflow.com/q/55868883/968003
	/// </remarks>
	public BacklogItemType?[]? Types { get; set; }

	/// <summary>
	///		Request items with specified states (new, closed, etc.)		
	/// </summary>
	/// <remarks>
	///		The (enum | null)[] type helps to prevent  "The value '' is invalid." validation error.
	///		See the problem description at https://stackoverflow.com/q/55868883/968003
	/// </remarks>
	public BacklogItemState?[]? States { get; set; }
		
	public string[]? Tags { get; set; }

	public string? Search { get; set; }

	public string? AssignedUserId { get; set; }

	public CurrentUserRelations? CurrentUserRelation { get; set; }

	public BacklogItemModification? UserModification { get; set; }

	public IDictionary<string,string>? CustomField { get; set; }
}