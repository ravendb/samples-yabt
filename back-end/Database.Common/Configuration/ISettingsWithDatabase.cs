namespace Raven.Yabt.Database.Common.Configuration
{
	public interface ISettingsWithDatabase
	{
		/// <summary>
		///		RavenDB connection parameters
		/// </summary>
		DatabaseSettings Database { get; }
		/// <summary>
		///		Raven database session settings
		/// </summary>
		DatabaseSessionSettings DatabaseSession { get; }
	}
}
