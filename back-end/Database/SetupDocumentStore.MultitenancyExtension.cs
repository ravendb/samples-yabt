using System;
using System.Linq;

using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.Helpers;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Database
{
	public static partial class SetupDocumentStore
	{
		private static partial void SetupMultitenancy(this IDocumentStore store, Func<string> tenantResolverFunc)
		{
			store.OnBeforeQuery += (_, args) => AddFilteringByTenantIdToSelectQueries(args.QueryCustomization, tenantResolverFunc());
			store.OnAfterConversionToEntity += (_, args) => ValidateTenantOnLoadingEntity(args.Entity, tenantResolverFunc());
		//	store.OnBeforeConversionToEntity += (sender, args) => { args. };
			store.OnBeforeDelete += (sender, args) => ValidateTenantOnDeletingEntity(sender, args.DocumentId, args.Entity, tenantResolverFunc());
			store.OnBeforeStore += (_, args) => AddTenantIdOnStoring(args.Entity, tenantResolverFunc());
		}

		private static void AddTenantIdOnStoring(object entity, string currentTenantId)
		{
			var entityType = entity.GetType(); 
			if (entityType.IsAssignableTo<ITenantedEntity>() != true) 
				return;
			entityType.GetProperty(nameof(ITenantedEntity.TenantId))?
					  .SetValue(entity, currentTenantId);
		}

		private static void AddFilteringByTenantIdToSelectQueries(IDocumentQueryCustomization customization, string currentTenantId)
		{
			// The lines below could have been simplified to `entityType = args.QueryCustomization.GetType().GetGenericArguments()[0]` 

			var type = customization.GetType();
			var entityType = type.GetInterfaces()
			                     .SingleOrDefault(i => 
				                     i.IsClosedTypeOf(typeof(IDocumentQuery<>)) || 
				                     i.IsClosedTypeOf(typeof(IAsyncDocumentQuery<>)))
			                     ?.GetGenericArguments()
			                     .SingleOrDefault();
			if (entityType?.IsAssignableTo<ITenantedEntity>() != true) 
				return;				
				
			// Add the "AND" to the the WHERE clause 
			// (the method has a check under the hood to prevent adding "AND" if the "WHERE" is empty)
			type.GetDeclaredMethod(
				    nameof(IDocumentQuery<object>.AndAlso))
			    .Invoke(customization, null);
			// Add "TenantId = 'Bla'" into the WHERE clause
			type.GetDeclaredMethod(
				    nameof(IDocumentQuery<object>.WhereEquals),
				    new[] { typeof(string), typeof(object), typeof(bool) })
			    .Invoke(
				    customization,
				    new object[]
				    {
					    nameof(ITenantedEntity.TenantId),
					    currentTenantId,
					    true,
				    });
		}

		private static void ValidateTenantOnLoadingEntity(object entity, string currentTenantId)
		{
			if (entity is ITenantedEntity tenantedEntity && tenantedEntity.TenantId != currentTenantId)
				throw new InvalidTenantException();
		}

		private static void ValidateTenantOnDeletingEntity(object? sender, string? entityId, object? entity, string currentTenantId)
		{
			if (entityId != null && entity == null)
			{
				if (sender == null)
					throw new NotSupportedException("Can't resolve the session object on deleting by ID");

			//	if (sender is AsyncDocumentSession asyncSession)
			//		entity = asyncSession.LoadAsync<dynamic>(entityId).Result;	// Get an exception on running 2 async operations simultaneously
				if (sender is DocumentSession session)
					entity = session.Load<dynamic>(entityId);

			}
			if (entity is ITenantedEntity tenantedEntity && tenantedEntity.TenantId != currentTenantId)
				throw new InvalidTenantException();
		}
	}
	
	public class InvalidTenantException: Exception { }
}
