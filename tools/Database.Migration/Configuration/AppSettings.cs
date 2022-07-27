using Raven.Yabt.Database.Common.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.Database.Migration.Configuration;

public class AppSettings : IAppSettingsWithDatabase
{
	/// <inheritdoc/>
	/// <remarks>
	///		Note that if it's not instantiated then it won't pick up settings set via environment variables or command line arguments 
	/// </remarks>
	public DatabaseSettings Database { get; } = new();

	/// <summary>
	///		Max waiting interval for rebuilding stale indexes.
	///		0 - infinite wait.
	/// </summary>
	public int MaxWaitingPeriodForRebuildingStaleIndexes { get; } = 0;
}