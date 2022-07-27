using System.Diagnostics.CodeAnalysis;

namespace Raven.Yabt.WebApi.Configuration.Settings;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class AppSettingsUserApiKey : UserAndTenantAndApiKeyDto
{
	public string UserId { get; private set; } = null!;
	public string TenantId { get; private set; } = null!;
}
public class UserAndTenantAndApiKeyDto
{
	public string UserName { get; protected set; } = null!;
	public string TenantName { get; protected set; } = null!;
	public string ApiKey { get; protected set; } = null!;
}