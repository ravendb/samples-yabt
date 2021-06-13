using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Newtonsoft.Json;

using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Session;
using Raven.Client.Json.Serialization.NewtonsoftJson;

using Raven.Yabt.Database.Common.Configuration;
using Raven.Yabt.Database.Common.Helpers;
using Raven.Yabt.Database.Models;
using Raven.Yabt.Database.Models.BacklogItems;

namespace Raven.Yabt.Database
{
	public static class SetupDocumentStore
	{
		/// <summary>
		///     Configure RavenDB Document Store
		/// </summary>
		public static void PreInitializeDocumentStore(this IDocumentStore store, Func<string>? tenantResolverFunc = null)
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
			//if (tenantResolverFunc != null)
			{
				store.OnBeforeQuery += (_, args) =>
				{
					//Type argument = args.QueryCustomization.GetType().GetGenericArguments()[0];
					//argument.IsAssignableFrom(typeof(ITenantData));
					var customization = args.QueryCustomization;
					//q.WhereEquals("TenantId", "1234");

					var type = customization.GetType();
					var entityType = type.GetInterfaces()
					                     .SingleOrDefault(
						                     i => i.IsClosedTypeOf(typeof(IDocumentQuery<>)) ||
							                     i.IsClosedTypeOf(typeof(IAsyncDocumentQuery<>)))
					                     ?.GetGenericArguments()
					                     .Single();
					if (entityType != null && entityType.IsAssignableTo<ITenantedEntity>())
					{
						(customization as IAsyncDocumentQuery<>).AndAlso()
						
						// Add the "AND" to the the WHERE clause 
						// (the method has a check under the hood to prevent adding "AND" if the "WHERE" is empty)
						type.GetDeclaredMethod("AndAlso").Invoke(customization, null);
						// Add "TenantId = 'Bla'" into the WHERE clause
						type.GetDeclaredMethod("WhereEquals", new[] { typeof(string), typeof(object) })
						    .Invoke(
							    customization,
							    new object[]
							    {
								    nameof(ITenantedEntity.TenantId),
								    "tenantResolverFunc()"
							    });
					}
				};
			}
		}

		/// <summary>
		///     Configure RavenDB Document Store
		/// </summary>
		/// <remarks>
		///		It DOESN'T create/update the indexes (by calling 'IndexCreation.CreateIndexes()'), as it may interfere with complex migration processes! Index creation/update should be called outside (along with the migration process).
		/// </remarks>
		public static IDocumentStore GetDocumentStore(DatabaseSettings settings,  Func<string>? tenantResolverFunc = null, Action<IDocumentStore>? customInit = null)
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
				
				store.PreInitializeDocumentStore(tenantResolverFunc);
				
				customInit?.Invoke(store);

				store.Initialize();
			}
			catch
			{
				store.Dispose();
				throw;
			}
			
			return store;
		}
	}
}