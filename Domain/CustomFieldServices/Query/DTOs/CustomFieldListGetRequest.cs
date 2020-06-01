using System.Collections.Generic;

namespace Raven.Yabt.Domain.CustomFieldServices.Query.DTOs
{
	public class CustomFieldListGetRequest
	{
		public IEnumerable<string>? Ids { get; set; }
	}
}
