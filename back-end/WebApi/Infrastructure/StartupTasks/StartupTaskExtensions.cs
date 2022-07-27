using Microsoft.Extensions.DependencyInjection;

using System.Linq;

namespace Raven.Yabt.WebApi.Infrastructure.StartupTasks;

/// <summary>
///		Helper extension methods for registering the shared context and startup tasks with the DI container:
/// </summary>
/// <remarks>
///		Based on https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-4-using-health-checks/
/// </remarks>
public static class StartupTaskExtensions
{
	private static readonly StartupTaskContext SharedContext = new StartupTaskContext();

	public static IServiceCollection AddStartupTasks(this IServiceCollection services)
	{
		// Add the singleton StartupTaskContext only once
		if (services.Any(x => x.ServiceType != typeof(StartupTaskContext)))
		{
			services.AddSingleton(SharedContext);
		}

		return services;
	}

	public static IServiceCollection AddStartupTask<T>(this IServiceCollection services) where T : class, IStartupTask
	{
		SharedContext.RegisterTask();

		return services
		       .AddStartupTasks()      // in case AddStartupTasks() hasn't been called
		       .AddHostedService<T>();
	}
}