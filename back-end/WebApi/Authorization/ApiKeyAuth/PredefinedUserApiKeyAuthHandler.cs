using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Raven.Yabt.WebApi.Configuration.Settings;

namespace Raven.Yabt.WebApi.Authorization.ApiKeyAuth;

/// <summary>
///		API key authentication handler for predefined users in 'appsettings.json'
/// </summary>
public class PredefinedUserApiKeyAuthHandler : AuthenticationHandler<PredefinedUserApiKeyAuthOptions>
{
	/// <summary>
	///		HTTP Header name for the API key.
	/// </summary>
	/// <remarks>
	///		All checks for that header name are case insensitive (https://stackoverflow.com/a/36287662/968003)
	/// </remarks>
	public const string ApiKeyHeaderName = "X-API-Key";

	private readonly AppSettingsUserApiKey[] _apiKeySettings;

	public PredefinedUserApiKeyAuthHandler(IOptionsMonitor<PredefinedUserApiKeyAuthOptions> options,
	                                       ILoggerFactory logger,
	                                       UrlEncoder encoder,
	                                       ISystemClock clock,
	                                       IOptionsMonitor<AppSettings> settings) : base(options, logger, encoder, clock)
	{
		_apiKeySettings = settings.CurrentValue.UserApiKey;
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		// If no X-Api-Key header, then return no result. Let other handlers (if present) handle the request

		if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
			return Task.FromResult(AuthenticateResult.NoResult());

		var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

		if (apiKeyHeaderValues.Count == 0 || string.IsNullOrWhiteSpace(providedApiKey))
			return Task.FromResult(AuthenticateResult.NoResult());

		// Explicitly fail if the expected API key hasn't been configured
		if (_apiKeySettings.Any() != true)
			throw new NullReferenceException("No API key specified in the 'appsettings.json'");

		// Validate the API key against the one in the settings
		var apiKeyUser = _apiKeySettings.SingleOrDefault(a => a.ApiKey == providedApiKey);

		if (apiKeyUser == null)
			return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));

		// The API key is valid
		// Create a new identity, add the name and ID claims so the ICurrentUserResolver handles it down the track

		var claims = new List<Claim>
		{
			new (CurrentUserResolver.UserIdClaimType,		apiKeyUser.UserId),
			new (CurrentTenantResolver.TenantIdClaimType,	apiKeyUser.TenantId),
		};
		var identity  = new ClaimsIdentity(claims, Options.AuthenticationType);
		var principal = new ClaimsPrincipal(new [] { identity });
		var ticket = new AuthenticationTicket(principal, Options.Scheme);

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}