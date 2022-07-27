using Raven.Yabt.Database.Common;
// ReSharper disable All

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

public record BacklogItemCustomFieldValue
{
	public string CustomFieldId { get; init; } = null!;
	public string Name { get; init; } = null!;
	public CustomFieldType Type { get; init; }
	public bool IsMandatory { get; init; }
	public object Value { get; init; } = null!;
}