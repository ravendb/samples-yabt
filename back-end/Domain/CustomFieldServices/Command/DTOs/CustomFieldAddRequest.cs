using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.CustomFieldServices.Command.DTOs
{
	public class CustomFieldAddRequest : CustomFieldUpdateRequest
	{
		public CustomFieldType Type { get; set; }
	}
}
