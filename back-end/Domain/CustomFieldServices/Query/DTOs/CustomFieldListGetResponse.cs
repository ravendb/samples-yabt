﻿using Raven.Yabt.Database.Common;
using Raven.Yabt.Domain.Common;

#nullable disable // Disable nullable check for a response DTO file

namespace Raven.Yabt.Domain.CustomFieldServices.Query.DTOs
{
	public class CustomFieldListGetResponse: ListResponseWithSanitisedIds
	{
		public string Name { get; set; }

		public CustomFieldType FieldType { get; set; }
		
		public bool IsMandatory { get; set; }
		
		/// <summary>
		///		Types of tickets the field is going to be used for (bugs, user stories or any type)
		/// </summary>
		public BacklogItemType?[]? BacklogItemTypes { get; set; }
	}
}
