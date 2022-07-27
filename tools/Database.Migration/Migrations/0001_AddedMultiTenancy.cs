using System;

using Raven.Client.Documents.Conventions;
using Raven.Migrations;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.Projects;
using Raven.Yabt.Database.Models.Users;

namespace Raven.Yabt.Database.Migration.Migrations;

/// <summary>
///		Converts some of existing entities to multi-tenanted and sets the default tenant
/// </summary>
/// <remarks>
///		https://github.com/migrating-ravens/RavenMigrations
/// </remarks>
[Migration(1)] 
public class AddedMultiTenancy: Raven.Migrations.Migration
{
	public override void Up()
	{
		// Create the first tenant/project
		string? firstTenantId = null;
		using (var session = DocumentStore.OpenSession())
		{
			var firstTenant = new Project
			{
				Name = "AspNetCore", 
				SourceUrl = "https://github.com/dotnet/aspnetcore", 
				LastUpdated = new DateTime(2021, 4, 24)
			};
			session.Store(firstTenant);
			session.SaveChanges();

			firstTenantId = firstTenant.Id.GetShortId();
			if (string.IsNullOrEmpty(firstTenantId))
				throw new Exception("Failed to create a default project");
		}

		// Set the ID of the first tenant/project in all tenanted entities
		foreach (var type in new[] {typeof(BacklogItem), typeof(CustomField), typeof(User)})
		{
			SetTenantForEntityRecords(type, firstTenantId);
		}
	}
		
	private void SetTenantForEntityRecords(Type entityType, string tenantId)
	{
		var pluralisedEntityName = DocumentConventions.DefaultGetCollectionName(entityType);
		if (string.IsNullOrEmpty(pluralisedEntityName))
			throw new Exception($"Failed to pluralise entity name {entityType.FullName}");
			
		PatchCollection($@"
				from {pluralisedEntityName!} as item
				update {{
					item.{nameof(ITenantedEntity.TenantId)} = '{tenantId}';
				}}
				");
	}
}