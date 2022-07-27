using Microsoft.Extensions.Hosting;

namespace Raven.Yabt.WebApi.Infrastructure.StartupTasks;

/// <summary>
///		A start-up task for the Web API
/// </summary>
/// <remarks>
///		Based on https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-4-using-health-checks/
/// </remarks>
public interface IStartupTask : IHostedService { }