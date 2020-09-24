namespace Raven.Yabt.WebApi.Configuration.Settings
{
#nullable disable
	public class AppSettings
	{
		public AppSettingsRavenDb Database { get; private set; }

		public AppSettingsUserApiKey[] UserApiKey { get; private set; }
	}
}
