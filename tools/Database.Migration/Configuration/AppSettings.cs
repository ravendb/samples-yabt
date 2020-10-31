using Raven.Yabt.Database.Configuration;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.Database.Migration.Configuration
{
#nullable disable
	public class AppSettings : ISettingsWithDatabase
	{
		public DatabaseSettings Database { get; private set; }
	}
}
