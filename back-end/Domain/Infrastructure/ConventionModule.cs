using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.Infrastructure
{
	internal class ConventionModule : ModuleRegistrationBase
	{
		protected override void Load(IServiceCollection services)
		{
			var types = typeof(BaseService<>).Assembly
							.GetTypes()
							.Where(t =>
									!t.IsAbstract
								&& (
									t.IsClosedTypeOf(typeof(BaseService<>))										// All services
								 || t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IPatchUpdateNotificationHandler)) // All notifications
								 || t.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IPatchOperationsExecuteAsync))	// Execute patch operations
								)
							).ToList();

			services.RegisterAsImplementedInterfaces(types, ServiceLifetime.Scoped);
			
			// Register 1 implementation for 2 interfaces
			services.AddScoped<PatchOperationsExecuteAsync>();
			services.AddScoped(x => (IPatchOperationsAddDeferred)x.GetService<PatchOperationsExecuteAsync>()!);
			services.AddScoped(x => (IPatchOperationsExecuteAsync)x.GetService<PatchOperationsExecuteAsync>()!);
		}
	}
}