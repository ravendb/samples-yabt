using Raven.Yabt.Database.Common.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.Database.Migration.Configuration
{
	public class AppSettings : ISettingsWithDatabase
	{
		/// <inheritdoc/>
		public DatabaseSettings Database { get; private set; } = null!;
		/// <inheritdoc/>
		public DatabaseSessionSettings DatabaseSession { get; private set; } = null!;
	}
}
