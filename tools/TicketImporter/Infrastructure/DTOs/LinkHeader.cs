using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs;

/// <summary>
///     Resolves "link" header
/// </summary>
/// <remarks>
///     Based on https://gist.github.com/pimbrouwers/8f78e318ccfefff18f518a483997be29
/// </remarks>
internal class LinkHeader
{
	public string? FirstLink { get; private set; }
	public string? PrevLink { get; private set; }
	public string? NextLink { get; private set; }
	public string? LastLink { get; private set; }

	public static LinkHeader? LinksFromHeader(HttpResponseMessage? httpResponse)
	{
		if (httpResponse != null && httpResponse.Headers.TryGetValues("link", out var values))
		{
			return LinksFromHeader(values.FirstOrDefault());
		}
		return null;
	}

	public static LinkHeader? LinksFromHeader(string? linkHeaderStr)
	{
		LinkHeader? linkHeader = null;

		if (!string.IsNullOrWhiteSpace(linkHeaderStr))
		{
			string[] linkStrings = linkHeaderStr.Split(',');

			if (linkStrings.Any())
			{
				linkHeader = new LinkHeader();

				foreach (string linkString in linkStrings)
				{
					var relMatch = Regex.Match(linkString, "(?<=rel=\").+?(?=\")", RegexOptions.IgnoreCase);
					var linkMatch = Regex.Match(linkString, "(?<=<).+?(?=>)", RegexOptions.IgnoreCase);

					if (relMatch.Success && linkMatch.Success)
					{
						string rel = relMatch.Value.ToUpper();
						string link = linkMatch.Value;

						switch (rel)
						{
							case "FIRST":
								linkHeader.FirstLink = link;
								break;
							case "PREV":
								linkHeader.PrevLink = link;
								break;
							case "NEXT":
								linkHeader.NextLink = link;
								break;
							case "LAST":
								linkHeader.LastLink = link;
								break;
						}
					}
				}
			}
		}

		return linkHeader;
	}
}