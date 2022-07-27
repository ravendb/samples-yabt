using System;
using System.Linq;

using Microsoft.AspNetCore.Http;

using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.WebApi.Authorization;

public class CurrentTenantResolver : ICurrentTenantResolver
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	/// <summary>
	///		Claim's name for 'tenant ID'
	/// </summary>
	/// <remarks>
	///		It's used to be "http://schemas.microsoft.com/identity/claims/tenantid", but now has been renamed to "tid".
	///		See https://docs.microsoft.com/en-us/dotnet/api/microsoft.identity.web.claimconstants
	/// </remarks>
	public const string TenantIdClaimType = "tid";

	public CurrentTenantResolver(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public string GetCurrentTenantId()
	{
		var tenantId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == TenantIdClaimType)?.Value;
			
		if (string.IsNullOrEmpty(tenantId))
			throw new UnauthorizedAccessException("Cannot resolve tenant ID");

		return tenantId;
	}
}