using System;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public class BacklogItemCommentGetResponse
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

		public UserReference Author { get; set; } = null!;  // Non-nullable
		public string Message { get; set; } = null!;    // Non-nullable
	}
}
