using Microsoft.AspNetCore.Authentication;

namespace Raven.Yabt.WebApi.Authorization.ApiKeyAuth;

/// <summary>
///		Options for the API key authentication handler for <see cref="PredefinedUserApiKeyAuthHandler"/>
/// </summary>
public class PredefinedUserApiKeyAuthOptions : AuthenticationSchemeOptions
{
	public const string DefaultScheme = "PredefinedUserApiKey";
	public string Scheme => DefaultScheme;
	public string AuthenticationType = DefaultScheme;
}