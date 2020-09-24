namespace Raven.Yabt.WebApi.Configuration.Settings
{
#nullable disable
	/// <summary>
	///     Connection to the Raven database
	/// </summary>
	public class AppSettingsRavenDb
	{
		/// <summary>
		///     URL of the RavenDB API
		/// </summary>
		public string[] RavenDbUrls { get; private set; }

		/// <summary>
		///     The path for Base64-encoded RavenDb certificate
		/// </summary>
		public string Certificate { get; private set; }

		/// <summary>
		///     The Database name
		/// </summary>
		public string DbName { get; private set; }

		/// <summary>
		///		The default time interval in seconds for waiting for the indexes to catch up with the saved changes
		///		after calling session.SaveChanges()
		/// </summary>
		public int WaitForIndexesAfterSaveChanges { get; private set; } = 30;

		/// <summary>
		///     Log a trace warning if session.Save() takes more than specified in seconds,
		///		but less than in <see cref="LogErrorIfSavingTakesMoreThan"/>.
		/// </summary>
		public int LogWarningIfSavingTakesMoreThan { get; private set; } = 15;

		/// <summary>
		///     Log an error if session.Save() takes more than specified in seconds.
		/// </summary>
		public int LogErrorIfSavingTakesMoreThan { get; private set; } = 30;
	}
}
