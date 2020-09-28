using System;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;

using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Raven.Yabt.Database.Models.BacklogItem;

namespace Raven.Yabt.Database
{
	public static class SetupDocumentStore
	{
		/// <summary>
		///     Configure RavenDB Document Store
		/// </summary>
		public static void PreInitializeDocumentStore(this IDocumentStore store)
		{
			store.Conventions.UseOptimisticConcurrency = true;

			// Added this so that when the property is missing in the DB, default values are assigned during serialization
			store.Conventions.Serialization = new NewtonsoftJsonSerializationConventions
				{
					CustomizeJsonSerializer = serializer => serializer.NullValueHandling = NullValueHandling.Ignore
				};

			// Set one collection for derived classes
			store.Conventions.FindCollectionName = type =>
				{
					if (typeof(BacklogItem).IsAssignableFrom(type))
						return DocumentConventions.DefaultGetCollectionName(typeof(BacklogItem)); // "BacklogItems";

					return DocumentConventions.DefaultGetCollectionName(type);
				};
		}

		/// <summary>
		///     Configure RavenDB Document Store
		/// </summary>
		public static IDocumentStore GetDocumentStore(string[] ravenDbUrl, string base64EncodedCertificate, string dbName)
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