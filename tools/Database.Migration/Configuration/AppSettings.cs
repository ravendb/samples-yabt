using Raven.Yabt.Database.Common.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.Database.Migration.Configuration
{
	public class AppSettings : IAppSettingsWithDatabase
	{
		/// <inheritdoc/>
		public DatabaseSettings Database { get; } = null!;

		/// <summary>
		///		Max waiting interval for rebuilding stale indexes.
		///		0 - infinite wait.
		/// </summary>
		public int MaxWaitingPeriodForRebuildingStaleIndexes { get; } = 0;
	}
}
