using System;
using System.Linq;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.TicketImporter.Services;

namespace Raven.Yabt.TicketImporter.Infrastructure;

/// <summary>
///		Get a random user as the current one
/// </summary>
internal class CurrentUserResolver : ICurrentUserResolver
{
	private readonly ISyncSeededUsersService _seededUser;

	public CurrentUserResolver(ISyncSeededUsersService seededUser)
	{
		_seededUser = seededUser;
	}

	/// <summary>
	///		Get a random generated user as the current one
	/// </summary>
	public string GetCurrentUserId() => _seededUser.GetGeneratedOrFetchedUsers().Result.OrderBy(x => Guid.NewGuid()).First().Id!;
}