
using Newtonsoft.Json;

using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
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
			store.Conventions.CustomizeJsonSerializer = serializer => serializer.NullValueHandling = NullValueHandling.Ignore;

			// Set one collection for derived classes
			store.Conventions.FindCollectionName = type =>
				{
					if (typeof(BacklogItem).IsAssignableFrom(type))
						return DocumentConventions.DefaultGetCollectionName(typeof(BacklogItem)); // "BacklogItems";

					return DocumentConventions.DefaultGetCollectionName(type);
				};
		}
	}
}