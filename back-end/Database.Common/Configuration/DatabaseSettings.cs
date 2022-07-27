// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Global
namespace Raven.Yabt.Database.Common.Configuration;

#nullable disable
/// <summary>
///     Connection to the Raven database
/// </summary>
public class DatabaseSettings
{
	/// <summary>
	///     URL of the RavenDB API
	/// </summary>
	public string[] RavenDbUrls { get; private set; }

	/// <summary>
	///     The path for Base64-encoded RavenDB certificate.
	///		Certificate is NOT required for non-secure connections (e.g. a local instance)
	/// </summary>
	public string Certificate { get; private set; }

	/// <summary>
	///     The Database name
	/// </summary>
	public string DbName { get; private set; }
		
	/// <summary>
	///     Flag triggering updating the indexes if 'true'.
	///		Ideally, it shouldn't be set in PROD as updating indexes is a migration concern,
	///		but setting it in dev environment makes live a bit easier by applying index updates on a start-up
	/// </summary>
	public bool UpdateIndexes { get; private set; }
}