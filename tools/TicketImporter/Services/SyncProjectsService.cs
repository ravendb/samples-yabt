using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.Projects;
using Raven.Yabt.TicketImporter.Configuration;

namespace Raven.Yabt.TicketImporter.Services;

internal interface ISyncProjectsService
{
	Task<(string id, string name)[]> SyncProjectsWithSettings();
	Task UpdateProjectTimestamp(string projectId);
}

internal class SyncProjectsService : ISyncProjectsService
{
	private readonly string[] _repos;
	private readonly IAsyncDocumentSession _dbSession;

	public SyncProjectsService(GitHubSettings settings, IAsyncDocumentSession dbSession)
	{
		_repos = settings.Repos;
		_dbSession = dbSession;
	}

	public async Task<(string id, string name)[]> SyncProjectsWithSettings()
	{
		var projects = await _dbSession.Query<Project>().ToListAsync();

		var projectIds = new Dictionary<string,string>();
			
		// Resolve the project IDs for the repos in the settings
		foreach (var repo in _repos)
		{
			var proj = projects.SingleOrDefault(p => p.SourceUrl.EndsWith(repo));
			if (proj == null)
			{
				proj = new Project { Name = repo, SourceUrl = $"https://github.com/{repo}" };
				await _dbSession.StoreAsync(proj);
				await _dbSession.SaveChangesAsync();
			}
			projectIds.Add(proj.Id.GetShortId()!, repo);
		}

		return projectIds.Select(p => (p.Key, p.Value)).ToArray();
	}

	public async Task UpdateProjectTimestamp(string projectId)
	{
		var fullId = _dbSession.Advanced.GetFullId<Project>(projectId);
		var proj = await _dbSession.LoadAsync<Project>(fullId);
		if (proj == null)
			throw new Exception($"Project #{projectId} not found");
			
		proj.LastUpdated = DateTime.UtcNow;
		await _dbSession.SaveChangesAsync();
	}
}