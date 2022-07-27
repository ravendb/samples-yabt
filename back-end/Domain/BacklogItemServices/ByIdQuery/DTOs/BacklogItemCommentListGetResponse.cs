using System;
using System.Collections.Generic;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

public class BacklogItemCommentListGetResponse
{
	public string Id { get; set; } = null!;
	public string Message { get; set; } = null!;

	public UserReference Author { get; set; } = null!;

	public DateTime Created { get; set; }
	public DateTime LastUpdated { get; set; }
		
	public IDictionary<string, string>? MentionedUserIds { get; set; }
}