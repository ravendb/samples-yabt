using Microsoft.Extensions.DependencyInjection;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.TicketImporter.Infrastructure;

namespace Raven.Yabt.TicketImporter.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Add <see cref="ICurrentUserResolver"/> services to the container
		/// </summary>
		public static IServiceCollection AddAndConfigureAuthentication(this IServiceCollection services)
		{
			services.AddScoped<ICurrentUserResolver, CurrentUserResolver>();
			return services;
		}
	}
}
