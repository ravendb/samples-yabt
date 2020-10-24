using System;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public class BacklogItemCommentGetResponse
	{
		public string Id { get; set; } = null!;  // Non-nullable
		public DateTime CreatedDate { get; set; }
		public DateTime ModifiedDate { get; set; }

		public UserReference Author { get; set; } = null!;  // Non-nullable
		public string Message { get; set; } = null!;    // Non-nullable
	}
}
