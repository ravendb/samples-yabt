using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.CustomFieldServices.Command.DTOs
{
	public class CustomFieldAddRequest
	{
		public string Name { get; set; } = null!;
		public CustomFieldType Type { get; set; }
	}
}
