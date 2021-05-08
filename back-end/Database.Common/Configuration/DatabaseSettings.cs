// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Global
namespace Raven.Yabt.Database.Common.Configuration
{
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
	}
}
