using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Database;

namespace Raven.Yabt.Domain.Infrastructure
{
	public static class DatabaseConnectionExtension
	{
		/// <summary>
		///     Register RavenDB connection (aka Document Store) and a DB session (aka unit of work)
		/// </summary>
		public static void AddAndConfigureRavenDb(this IServiceCollection services, string[] ravenDbUrls, string base64EncodedCertificate, string dbName)
		{
			// Register the document store as single instance, initializing it on first use
			services.AddSingleton(x => GetDocumentStore(ravenDbUrls, base64EncodedCertificate, dbName));

			// Register a session
			services.AddScoped(c => c.GetService<IDocumentStore>().OpenAsyncSession());
		}

		/// <summary>
		///     Get Document Store connected to a non-embedded instance of RavenDB.
		///		Note: it DOESN'T create/update the indexes (by calling 'IndexCreation.CreateIndexes()'), as it may interfier complex migration processes! Index creation/update should be called outside (along with the migration process).
		/// </summary>
		private static IDocumentStore GetDocumentStore(string[] ravenDbUrls, string base64EncodedCertificate, string dbName)
		{
			IDocumentStore store = null!;
			try
			{
				// Connect to RavenDB using authentication via certificate
				if (!string.IsNullOrEmpty(base64EncodedCertificate) &&
					(ravenDbUrls.Length > 1
					  || ravenDbUrls.FirstOrDefault()?.Contains("localhost:8888", StringComparison.InvariantCultureIgnoreCase) == false)
				   )
				{
					byte[] certificate = Convert.FromBase64String(base64EncodedCertificate);
					store = new DocumentStore
						{
							Certificate = new X509Certificate2(certificate),
							Urls = ravenDbUrls,
							Database = dbName
						};
				}
				// Connect to RavenDB without authentication
				else
				{
					store = new DocumentStore
						{
							Urls = ravenDbUrls,
							Database = dbName
						};
				}

				store.PreInitializeDocumentStore();

				store.Initialize();

				return store;
			}
			catch
			{
				store?.Dispose();
				throw;
			}
		}
	}
}
