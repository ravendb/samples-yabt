using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;

namespace Raven.Yabt.TicketImporter.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register the document store as single instance, initializing it on first use
		/// </summary>
		public static IServiceCollection AddAndConfigureDatabaseSessionForImport(this IServiceCollection services)
		{
			return services.AddScoped(c =>
				{
					var session = c.GetRequiredService<IDocumentStore>().OpenAsyncSession();
						session.Advanced.WaitForIndexesAfterSaveChanges();  // Wait on each change to avoid adding WaitForIndexing() in each test
					return session;
				});
		}
	}
}
