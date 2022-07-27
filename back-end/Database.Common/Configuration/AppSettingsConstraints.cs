namespace Raven.Yabt.Database.Common.Configuration;

/// <summary>
///		Constraint on naming RavenDB connection parameters  
/// </summary>
public interface IAppSettingsWithDatabase
{
	/// <summary>
	///		RavenDB connection parameters
	/// </summary>
	DatabaseSettings Database { get; }
}