using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.Infrastructure
{
	internal class ConventionModule : ModuleRegistration
	{
		protected override void Load(IServiceCollection services)
		{
			var types = typeof(BaseService<>).Assembly
							.GetTypes()
							.Where(t =>
									!t.IsAbstract
								&& (
									t.IsClosedTypeOf(typeof(BaseService<>))													// All services
								 || t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IPatchUpdateNotificationHandler)) // All notifications
								 || t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IPatchOperationsExecuteAsync))	// Execute patch operations
								)
							).ToList();

			services.RegisterAsImplementedInterfaces(types, ServiceLifetime.Scoped);
		}
	}
}