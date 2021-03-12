using Raven.Yabt.Database.Common.BacklogItem;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs
{
	public record BacklogRelationshipAction
	{
		public string BacklogItemId { get; init; }
		
		public BacklogRelationshipType RelationType  { get; init; }
		
		public BacklogRelationshipActionType ActionType  { get; init; }
	}
}
