using System.Threading;
using System.Threading.Tasks;

using Raven.Yabt.TicketImporter.Infrastructure;
using Raven.Yabt.TicketImporter.Services;

namespace Raven.Yabt.TicketImporter;

internal class MainWorker
{
	private readonly ISyncProjectsService _syncProjectsService;
	private readonly ICustomFieldService _syncCustomFieldsService;
	private readonly ISyncSeededUsersService _seededUserService;
	private readonly ISyncGitHubIssuesService _seedIssuesService;
	private readonly ICurrentTenantSetter _currentTenantSetter;

	public MainWorker ( ISyncProjectsService syncProjectsService,
	                    ICustomFieldService syncCustomFieldsService,
	                    ISyncSeededUsersService seededUserService,
	                    ISyncGitHubIssuesService seedIssuesService,
	                    ICurrentTenantSetter currentTenantSetter)
	{
		_syncProjectsService = syncProjectsService;
		_syncCustomFieldsService = syncCustomFieldsService;
		_seededUserService = seededUserService;
		_seedIssuesService = seedIssuesService;
		_currentTenantSetter = currentTenantSetter;
	}

	public async Task Run(CancellationToken cancellationToken)
	{
		var projects = await _syncProjectsService.SyncProjectsWithSettings();
			
		// Iterate through the GitHub repos and import tickets with comments  
		foreach (var (id, repo) in projects)
		{
			// Set the current tenant for the methods below
			_currentTenantSetter.SetCurrentTenantId(id);
				
			// Generate or fetch the users
			var userReferences = await _seededUserService.GetGeneratedOrFetchedUsers();
				
			// Generate or fetch the Custom Field for preserving the reference to the original ticket
			var customFieldId = await _syncCustomFieldsService.GenerateOrFetchUrlCustomField();
			var gitHubUrls = await _syncCustomFieldsService.GetGitHubUrlsForExistingTickets(customFieldId);

			await _seedIssuesService.CreateTicketsForGitHubIssues(repo, userReferences, customFieldId, gitHubUrls, cancellationToken);

			await _syncProjectsService.UpdateProjectTimestamp(id);
		}
	}
}