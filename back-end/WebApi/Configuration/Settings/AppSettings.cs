using Raven.Yabt.Database.Configuration;
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.WebApi.Configuration.Settings
{
#nullable disable
	public record AppSettings : ISettingsWithDatabase
	{
		/// <summary>
		///		RavenDB connection parameters
		/// </summary>
		public DatabaseSettings Database { get; private set; }

		/// <summary>
		///		Array of API keys mapped to users
		/// </summary>
		public AppSettingsUserApiKey[] UserApiKey { get; private set; }
		
		/// <summary>
		///		';'-separated list of enabled consumers of the API
		/// </summary>
		public string CorsOrigins { get; private set; }
		
		/// <summary>
		///		Folder with compiled SPA hosted by Kestrel
		/// </summary>
		public string SpaRootPath { get; private set; }
	}
}
