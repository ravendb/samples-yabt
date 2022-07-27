using System.Linq;

using Raven.Yabt.Database.Common.References;

#nullable disable // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;

public class CustomFieldReferenceDto : IEntityReference
{
	private string _id;

	public string Id
	{
		get => _id;
		set => _id = value?.Split('/').Last();
	}

	public string Name { get; set; } = null!;   // Non-nullable
}