namespace Raven.Yabt.Database.Common.Configuration
{
	/// <summary>
	///		Constraint on naming config parameters in 2 different `appsettings.json` files  
	/// </summary>
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
