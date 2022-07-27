using System.Linq;

namespace Raven.Yabt.Domain.Common;

public class ListResponseWithSanitisedIds
{
	private string? _id;
	public string? Id
	{
		get => _id;
		set => _id = value?.Split('/').Last();
	}
}