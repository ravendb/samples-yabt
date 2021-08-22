// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace Raven.Yabt.WebApi.Configuration.Settings
{
#nullable disable
	public class AppSettingsUserApiKey : UserAndTenantAndApiKeyDto
	{
		public string UserId { get; private set; }
		public string TenantId { get; private set; }
	}
	public class UserAndTenantAndApiKeyDto
	{
		public string UserName { get; protected set; }
		public string TenantName { get; protected set; }
		public string ApiKey { get; protected set; }
	}
}
