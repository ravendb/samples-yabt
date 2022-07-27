using System;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;

using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Indexes;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Raven.Yabt.Database.Common.Configuration;

namespace Raven.Yabt.Database.Infrastructure;

public static class SetupDocumentStore
{
	/// <summary>
	///     Configure RavenDB Document Store
	/// </summary>
	public static void PreInitializeDocumentStore(this IDocumentStore store)
	{
		store.Conventions.UseOptimisticConcurrency = true;

		// If a property is missing in the DB, default values are assigned during serialization
		store.Conventions.Serialization = new NewtonsoftJsonSerializationConventions
		{
			CustomizeJsonSerializer = serializer => serializer.NullValueHandling = NullValueHandling.Ignore
		};

		// Set one collection for derived classes
		store.Conventions.FindCollectionName = type =>
		{
			if (typeof(Models.BacklogItems.BacklogItem).IsAssignableFrom(type))
				return DocumentConventions.DefaultGetCollectionName(typeof(Models.BacklogItems.BacklogItem)); // "BacklogItems";

			return DocumentConventions.DefaultGetCollectionName(type);
		};
	}

	/// <summary>
	///     Configure RavenDB Document Store
	/// </summary>
	/// <remarks>
	///		It DOESN'T create/update the indexes (by calling 'IndexCreation.CreateIndexes()'), as it may interfere with complex migration processes! Index creation/update should be called outside (along with the migration process).
	/// </remarks>
	public static IDocumentStore GetDocumentStore(DatabaseSettings settings, Action<IDocumentStore>? customInit = null)
	{
		var store = new DocumentStore
		{
			Urls = settings.RavenDbUrls,
			Database = settings.DbName,
		};
		try
		{
			// A public/secure instance of RavenDB requires authentication via certificate
			if (!string.IsNullOrEmpty(settings.Certificate))
				store.Certificate = new X509Certificate2(Convert.FromBase64String(settings.Certificate));
				
			store.PreInitializeDocumentStore();
				
			customInit?.Invoke(store);

			store.Initialize();
				
			if (settings.UpdateIndexes)
				IndexCreation.CreateIndexesAsync(typeof(SetupDocumentStore).Assembly, store, null, store.Database);
		}
		catch
		{
			store.Dispose();
			throw;
		}
			
		return store;
	}
}