using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Raven.Yabt.TicketImporter.Configuration;
using Raven.Yabt.TicketImporter.Helpers;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Infrastructure;

internal interface IGitHubService
{
	IAsyncEnumerable<IssueResponse[]> GetIssuesAsync(string repoName, Func<IssueResponse, bool>? validateIssue = null, CancellationToken cancellationToken = default);
}
	
internal class GitHubService : IGitHubService
{
	private readonly HttpClient _httpClient;
	private readonly GitHubSettings _gitHubSettings;
	private readonly ILogger<GitHubService> _logger;

	public GitHubService(HttpClient httpClient, GitHubSettings gitHubSettings, ILogger<GitHubService> logger)
	{
		_httpClient = httpClient;
		_gitHubSettings = gitHubSettings;
		_logger = logger;
	}

	public IAsyncEnumerable<IssueResponse[]> GetIssuesAsync(string repoName, Func<IssueResponse, bool>? validateIssue = null, CancellationToken cancellationToken = default)
	{
		var builder = new QueryBuilder(DtoConversion.ToDictionary(new IssuesRequest()));
		var requestString = string.Concat($"repos/{repoName}/issues", builder.ToString());

		async Task<IssueResponse[]> IssueProcessing(IssueResponse[] issues)
		{
			var issueQuery = issues.Where(i => !i.IsPullRequest && validateIssue?.Invoke(i) != false).AsQueryable();
			foreach (var issue in issueQuery.Where(i => i.CommentsCount > 0))
			{
				issue.Comments = new List<CommentResponse>();
				await foreach (var comments in GetList<CommentResponse>(issue.CommentsUrl, _gitHubSettings.MaxImportedIssues, cancellationToken))
				{
					issue.Comments.AddRange(comments);
				}
			}
			return issueQuery.ToArray();
		}

		return GetList<IssueResponse>(requestString, _gitHubSettings.MaxImportedIssues, cancellationToken, IssueProcessing);
	}

	private async IAsyncEnumerable<T[]> GetList<T>(string? startUrl, int maxNumber, [EnumeratorCancellation] CancellationToken cancellationToken, Func<T[], Task<T[]>>? processing = null)
	{
		var counter = 0;
		while (!string.IsNullOrEmpty(startUrl))
		{
			if (cancellationToken.IsCancellationRequested || counter > maxNumber)
				yield break;

			using var request = new HttpRequestMessage(HttpMethod.Get, startUrl);
			using var httpResponse = await _httpClient.SendAsync(request, cancellationToken /* throws exception on cancellation */);

			if (httpResponse.IsSuccessStatusCode)
			{
				startUrl = null;
				var links = LinkHeader.LinksFromHeader(httpResponse);
				if (!string.IsNullOrEmpty(links?.NextLink))
					startUrl = links.NextLink;

				await using var stream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
				var responseArray = await JsonSerializer.DeserializeAsync<T[]>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, cancellationToken);

				if (responseArray == null)
				{
					LogResponseError("Failed to parse response", httpResponse);
					yield break;
				}

				if (processing != null)
					responseArray = await processing(responseArray);

				counter += responseArray.Length;

				yield return responseArray;
			}
			else
				LogResponseError("Failed to fetch a page", httpResponse);
		}
	}

	private void LogResponseError(string err, HttpResponseMessage response)
	{
		_logger.LogError(err, response.StatusCode, response.Content);
	}
}