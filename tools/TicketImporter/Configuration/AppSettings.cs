#nullable disable
namespace Raven.Yabt.TicketImporter.Configuration
{
	internal class AppSettings
	{
		/// <summary>
		///		The GitHub settings
		/// </summary>
		public GitHubSettings GitHub { get; private set; }
	}

	internal class GitHubSettings
	{
		/// <summary>
		///		The repos for importing
		/// </summary>
		public string[] Repos { get; private set; }

		/// <summary>
		///		GitHub login: Client ID
		/// </summary>
		public string ClientId { get; private set; }
		/// <summary>
		///		GitHub login: Client Secret
		/// </summary>
		public string ClientSecret { get; private set; }
	}
}
