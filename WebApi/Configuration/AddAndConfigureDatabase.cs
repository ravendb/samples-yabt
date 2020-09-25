using System;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Yabt.Database;
using Raven.Yabt.WebApi.Configuration.Settings;

namespace Raven.Yabt.WebApi.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		/// <summary>
		///		Register the document store as single instance, initializing it on first use
		/// </summary>
		public static void AddAndConfigureDatabase(this IServiceCollection services)
		{
			services.AddSingleton(x =>
				{
					var config = x.GetService<AppSettingsRavenDb>();
					var store = GetDocumentStore(config.RavenDbUrls, config.Certificate, config.DbName);
						store.PreInitializeDocumentStore();

					return store.Initialize();
				});
			services.AddScoped(c =>
				{
					var session = c.GetService<IDocumentStore>().OpenAsyncSession();
						session.Advanced.WaitForIndexesAfterSaveChanges();  // Wait on each change to avoid adding WaitForIndexing() in each test
					return session;
				});
		}

		/// <summary>
		///     Configure RavenDB Document Store
		/// </summary>
		private static IDocumentStore GetDocumentStore(string[] ravenDbUrl, string base64EncodedCertificate, string dbName)
		{
			// Connect to a public RavenDB (authentication via certificate)
			if (!string.IsNullOrEmpty(base64EncodedCertificate))
			{
				byte[] certificate = Convert.FromBase64String(base64EncodedCertificate);
				return new DocumentStore
					{
						Certificate = new X509Certificate2(certificate),
						Urls = ravenDbUrl,
						Database = dbName
					};
			}
			else    // Connect to local RavenDB (no authentication)
				return new DocumentStore
					{
						Urls = ravenDbUrl,
						Database = dbName
					};
		}
	}
}
