using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

public record BacklogRelationshipAction
{
	public string BacklogItemId { get; init; } = null!;
		
	public BacklogRelationshipType RelationType  { get; init; }
		
	public ListActionType ActionType  { get; init; }
}