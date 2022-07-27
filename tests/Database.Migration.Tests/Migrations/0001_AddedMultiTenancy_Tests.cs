using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Migrations;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Migration.Tests;
using Raven.Yabt.Database.Models;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.Projects;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Database.Models.Users.Indexes;

using Xunit;

namespace Raven.Yabt.Database.Migration.Migrations;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AddedMultiTenancy_Test: ConfigureTestEnvironment
{
	[Fact]
	public async Task Running_Multitenancy_Update_Adds_TenantIds_And_Creates_Project_Record()
	{
		// GIVEN 3 different records in the DB
		var userId = await CreateUser();
		var ticketId = await CreateTicket();
		var fieldId = await CreateCustomField();
			
		// WHEN run migration
		var migration = new AddedMultiTenancy();
		migration.Setup(DbSession.Advanced.DocumentStore, new MigrationOptions(), CreateLogger<AddedMultiTenancy_Test>());
		migration.Up();
			
		// Clear cache to force the session update the entities from the DB
		DbSession.Advanced.Clear();

		// THEN 
		//	a Project entity created
		var projectId = await GetProjectId();
		Assert.False(string.IsNullOrEmpty(projectId));
		//  and all tenanted existing entities get the tenant Id
		Assert.Equal(projectId, await GetTenantId<User>(userId));
		Assert.Equal(projectId, await GetTenantId<BacklogItem>(ticketId));
		Assert.Equal(projectId, await GetTenantId<CustomField>(fieldId));
	}

	private async Task<string> CreateUser()
	{
		var record = new User { FirstName = "Homer", LastName = "Simpson" };
		await DbSession.StoreAsync(record);
		await DbSession.SaveChangesAsync();

		return record.Id;
	}
	private async Task<string> CreateTicket()
	{
		var record = new BacklogItemBug { Title = "Test"};
		await DbSession.StoreAsync(record);
		await DbSession.SaveChangesAsync();

		return record.Id;
	}
	private async Task<string> CreateCustomField()
	{
		var record = new CustomField { Name = "Field", FieldType = CustomFieldType.Date };
		await DbSession.StoreAsync(record);
		await DbSession.SaveChangesAsync();

		return record.Id;
	}

	private async Task<string?> GetProjectId()
	{
		var projects = await DbSession.Query<Project>().ToListAsync();
		return projects.SingleOrDefault()?.Id.GetShortId();
	}
		
	private async Task<string> GetTenantId<T>(string fullId) where T : ITenantedEntity
	{
		var entity = await DbSession.LoadAsync<T>(fullId);
		return entity!.TenantId;
	}
}