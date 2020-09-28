using Raven.Yabt.Database.Configuration;

namespace Raven.Yabt.WebApi.Configuration.Settings
{
#nullable disable
	public class AppSettings : ISettingsWithDatabase
	{
		public DatabaseSettings Database { get; private set; }

		public AppSettingsUserApiKey[] UserApiKey { get; private set; }
	}
}
