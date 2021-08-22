using Raven.Yabt.Database.Common.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Raven.Yabt.WebApi.Configuration.Settings
{
#nullable disable
	public record AppSettings : IAppSettingsWithDatabase
	{
		/// <inheritdoc/>
		public DatabaseSettings Database { get; private set; }
		
		/// <summary>
		///		Raven database session settings
		/// </summary>
		public DatabaseSessionSettings DatabaseSession { get; private set; }

		/// <summary>
		///		Array of API keys mapped to users
		/// </summary>
		public AppSettingsUserApiKey[] UserApiKey { get; private set; }
		
		/// <summary>
		///		';'-separated list of enabled consumers of the API
		/// </summary>
		public string CorsOrigins { get; private set; }
	}
}
