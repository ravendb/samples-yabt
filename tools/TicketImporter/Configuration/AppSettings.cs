#nullable disable
using Raven.Yabt.Database.Common.Configuration;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace Raven.Yabt.TicketImporter.Configuration;

internal class AppSettings : IAppSettingsWithDatabase
{
	/// <summary>
	///		The GitHub settings
	/// </summary>
	public GitHubSettings GitHub { get; private set; }

	/// <inheritdoc/>
	public DatabaseSettings Database { get; private set; }

	public GeneratedRecordsSettings GeneratedRecords { get; private set; }
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

	/// <summary>
	/// 	Maximum quantity of imported issues. Default - all issues
	/// </summary>
	public int MaxImportedIssues { get; private set; } = int.MaxValue;
}

internal class GeneratedRecordsSettings
{
	/// <summary>
	/// 	Number of generated users
	/// </summary>
	public int NumberOfUsers { get; private set; } = 100;

	/// <summary>
	///		Percentage of the tickets that have 'Related Items'
	/// </summary>
	public double PartOfTicketsWithRelatedItems { get; private set; } = .1;
	/// <summary>
	///		Percentage of assigned tickets
	/// </summary>
	public double PartOfAssignedTickets { get; private set; } = .3;
}