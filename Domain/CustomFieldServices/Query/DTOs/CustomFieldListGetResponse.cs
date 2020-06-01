using System.Linq;

using Raven.Yabt.Database.Common;

#nullable disable // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.CustomFieldServices.Query.DTOs
{
	public class CustomFieldListGetResponse
	{
		private string _id;

		public string Id
		{
			get { return _id; }
			set { _id = value?.Split('/').Last(); }
		}

		public string Name { get; set; }

		public CustomFieldType FieldType { get; set; }
	}
}
