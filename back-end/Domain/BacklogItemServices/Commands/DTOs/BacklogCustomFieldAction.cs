using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs
{
	public class BacklogCustomFieldAction
	{
		public string CustomFieldId { get; init; }
		
		public object Value  { get; init; }
		
		public ListActionType ActionType  { get; init; }
	}
}
