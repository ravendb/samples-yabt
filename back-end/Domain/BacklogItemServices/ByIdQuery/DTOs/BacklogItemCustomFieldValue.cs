using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public record BacklogItemCustomFieldValue
	{
		public string CustomFieldId { get; init; }
		public string Name { get; init; }
		public CustomFieldType Type { get; init; }
		public bool IsMandatory { get; init; }
		public object Value { get; init; }
	}
}
