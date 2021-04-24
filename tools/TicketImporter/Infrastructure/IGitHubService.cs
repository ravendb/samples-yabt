using System;
using System.Collections.Generic;
using System.Threading;

using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Infrastructure
{
	internal interface IGitHubService
	{
		IAsyncEnumerable<IssueResponse[]> GetIssues(string repoName, int maxNumber = int.MaxValue, Func<IssueResponse, bool>? validateIssue = null, CancellationToken cancellationToken = default);
	}
}
