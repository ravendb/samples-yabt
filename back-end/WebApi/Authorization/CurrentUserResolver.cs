using System;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.WebApi.Authorization
{
	public class CurrentUserResolver : ICurrentUserResolver
	{
		private readonly IHttpContextAccessor _httpContextAccessor;

		public const string UserIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";

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
}