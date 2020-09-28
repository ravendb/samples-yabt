using Raven.Yabt.Database.Configuration;

namespace Raven.Yabt.Database.Migration.Configuration
{
#nullable disable
	public class AppSettings : ISettingsWithDatabase
	{
		public DatabaseSettings Database { get; private set; }
	}
}
