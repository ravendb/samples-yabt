using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Database.Models.CustomField
{
	public class CustomField : IEntity
	{
		/// <summary>
		///		The record ID
		/// </summary>
		/// <remarks>
		///		Set by Raven Client. Can be temporarily null before passed to the DocumentSession.Store() method
		/// </remarks>
		public string Id { get; set; } = null!;

		public string Name { get; set; } = null!;

		public CustomFieldType FieldType { get; set; }
	}
}
