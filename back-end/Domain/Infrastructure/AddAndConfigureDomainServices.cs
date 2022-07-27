using System.Linq;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.Common;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static void AddAndConfigureDomainServices(this IServiceCollection services, bool addAndConfigureDatabase)
	{
		if (addAndConfigureDatabase)
		{
			services.AddAndConfigureDatabase();
			services.AddAndConfigureDatabaseTenantedSession(x => x.GetRequiredService<ICurrentTenantResolver>().GetCurrentTenantId);
		}

		var types = typeof(BaseDbService).Assembly
		                                 .GetTypes()
		                                 .Where(t =>
					                                 !t.IsAbstract
					                                 && t.IsAssignableTo<BaseDbService>()		// All services
			                                 ).ToList();

		services.RegisterAsImplementedInterfaces(types, ServiceLifetime.Scoped);
	}
}