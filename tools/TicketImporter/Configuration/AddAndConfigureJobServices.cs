using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.TicketImporter.Services;

namespace Raven.Yabt.TicketImporter.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register the services to do the job
	/// </summary>
	public static IServiceCollection AddAndConfigureJobServices(this IServiceCollection services)
	{
		return services.AddHostedService<HostedServiceWrapper>()
		               .AddScoped<ISyncProjectsService, SyncProjectsService>()
		               .AddScoped<ISyncSeededUsersService, SyncSeededUsersService>()
		               .AddScoped<ISyncGitHubIssuesService, SyncGitHubIssuesService>()
		               .AddScoped<ICustomFieldService, CustomFieldService>()
		               .AddScoped<MainWorker>();
	}
}