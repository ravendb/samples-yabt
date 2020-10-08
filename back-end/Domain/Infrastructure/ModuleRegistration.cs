using Microsoft.Extensions.DependencyInjection;

namespace Raven.Yabt.Domain.Infrastructure
{
	/// <summary>
	///		Base class for user-defined modules. Modules can add a set of related components
	///		to a container (<see cref="Load"/>)
	/// </summary>
	public abstract class ModuleRegistration
	{
		/// <summary>
		///		Apply the module to the component registry.
		/// </summary>
		public void Configure(IServiceCollection services)
		{
			Load(services);
		}

		protected abstract void Load(IServiceCollection services);
	}
}