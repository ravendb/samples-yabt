﻿using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Database.Models.CustomFields
{
	public class CustomField : BaseEntity
	{
		public string Name { get; set; } = null!;

		/// <summary>
		///		Type of the custom field determines how to process the associated value
		/// </summary>
		public CustomFieldType FieldType { get; set; }

		public bool IsMandatory { get; set; }
		
		/// <summary>
		///		Types of tickets the field is going to be used for (bugs, user stories or any type)
		/// </summary>
		public BacklogItemType?[]? BacklogItemTypes { get; set; }
	}
}