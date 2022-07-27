using System;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.WebApi.Authorization;

public class CurrentUserResolver : ICurrentUserResolver
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	/// <summary>
	///		Claim's name for 'user ID'
	/// </summary>
	/// <remarks>
	///		It's used to be "http://schemas.microsoft.com/identity/claims/objectidentifier", but now has been renamed to "oid".
	///		See https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.web.claimconstants
	/// </remarks>
	public const string UserIdClaimType = "oid";

	public CurrentUserResolver(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public string GetCurrentUserId()
	{
		var userId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == UserIdClaimType)?.Value;
			
		if (string.IsNullOrEmpty(userId))
			throw new UnauthorizedAccessException("Cannot resolve user ID");

		return userId;
	}
}