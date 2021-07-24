using Raven.Yabt.Database.Common.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.Database.Migration.Configuration
{
	public class AppSettings : ISettingsWithDatabase
	{
		/// <inheritdoc/>
		public DatabaseSettings Database { get; } = null!;
		/// <inheritdoc/>
		public DatabaseSessionSettings DatabaseSession { get; } = null!;

		/// <summary>
		///		Max waiting interval for rebuilding stale indexes.
		///		0 - infinite wait.
		/// </summary>
		public int MaxWaitingPeriodForRebuildingStaleIndexes { get; } = 0;
	}
}
