using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Raven.Yabt.TicketImporter.Helpers;
using Raven.Yabt.TicketImporter.Infrastructure.DTOs;

namespace Raven.Yabt.TicketImporter.Infrastructure
{
	internal class GitHubService : IGitHubService
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<GitHubService> _logger;

		public GitHubService(HttpClient httpClient, ILogger<GitHubService> logger)
		{
			_httpClient = httpClient;
			_logger = logger;
		}

		public IAsyncEnumerable<IssueResponse[]> GetIssues(string repoName, int maxNumber = int.MaxValue, CancellationToken cancellationToken = default)
		{
			var builder = new QueryBuilder(DtoConversion.ToDictionary(new IssuesRequest()));
			var requestString = string.Concat($"repos/{repoName}/issues", builder.ToString());

			async Task<IssueResponse[]> IssueProcessing(IssueResponse[] issues)
				{
					foreach (var issue in issues.Where(i => i.CommentsCount > 0))
					{
						issue.Comments = new List<CommentResponse>();
						await foreach (var comments in GetList<CommentResponse>(issue.CommentsUrl, maxNumber, cancellationToken))
						{
							issue.Comments.AddRange(comments);
						}
					}
					return issues.Where(i => i.Labels.All(l => l.Name != "auto-merge")).ToArray();
				}

			return GetList<IssueResponse>(requestString, maxNumber, cancellationToken, IssueProcessing);
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
}
