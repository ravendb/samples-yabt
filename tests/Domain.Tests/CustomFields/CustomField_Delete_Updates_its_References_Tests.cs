using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Command;
using Raven.Yabt.Domain.CustomFieldServices.Command.DTOs;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.CustomFields;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CustomField_Delete_Updates_its_References_Tests : ConfigureTestEnvironment
{
	private readonly ICustomFieldCommandService _commandService;
	private readonly IBacklogItemCommandService _commandTicketService;
	private readonly IBacklogItemByIdQueryService _queryTicketService;

	public CustomField_Delete_Updates_its_References_Tests()
	{
		_commandService = Container.GetService<ICustomFieldCommandService>()!;
		_commandTicketService = Container.GetService<IBacklogItemCommandService>()!;
		_queryTicketService = Container.GetService<IBacklogItemByIdQueryService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		var userResolver = Substitute.For<IUserReferenceResolver>();
		var currentUser = new UserReference { Id = "1", Name = "H. Simpson", FullName = "Homer Simpson" };
		userResolver.GetCurrentUserReference().Returns(currentUser);
		services.AddScoped(_ => userResolver);
	}

	[Fact]
	private async Task Deleted_CustomField_Disappears_From_BacklogItems_Using_This_Field()
	{
		// GIVEN 2 tickets referencing 2 custom fields
		var customFieldId1 = await CreateSampleCustomField();
		var customFieldId2 = await CreateSampleCustomField();
		var (ticketId1, _) = await CreateSampleBug(customFieldId1, customFieldId2);
		var (ticketId2, _) = await CreateSampleBug(customFieldId1, customFieldId2);

		// WHEN deleting 1 custom field
		await _commandService.Delete(customFieldId1);
		await SaveChanges();

		// THEN
		// The custom fields used in the 2 tickets also get deleted
		var ticket1 = (await _queryTicketService.GetById(ticketId1!)).Value;
		Assert.Equal(1, ticket1.CustomFields!.Count);
		Assert.Equal(customFieldId2, ticket1.CustomFields[0].CustomFieldId);
		var ticket2 = (await _queryTicketService.GetById(ticketId2!)).Value;
		Assert.Equal(1, ticket2.CustomFields!.Count);
		Assert.Equal(customFieldId2, ticket2.CustomFields[0].CustomFieldId);
	}

	private async Task<string> CreateSampleCustomField()
	{
		var dto = new CustomFieldAddRequest
		{
			Name = "Test Custom Field"+GetRandomString(),
			FieldType = Database.Common.CustomFieldType.Text
		};
		var (fieldRef, _) = await _commandService.Create(dto);
		await SaveChanges();

		return fieldRef.Id;
	}

	private async Task<BacklogItemReference> CreateSampleBug(params string[] customFieldId)
	{
		var dto = new BugAddUpdRequest
		{
			Title = "Test Bug",
			Severity = BugSeverity.Critical,
			Priority = BugPriority.P1,
			ChangedCustomFields = customFieldId.Select(id => new BacklogCustomFieldAction { CustomFieldId = id, ObjValue = "Test", ActionType = ListActionType.Add}).ToList()
		};
		var ticketAddedRef = await _commandTicketService.Create(dto);
		if (!ticketAddedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");
		await SaveChanges();

		return ticketAddedRef.Value;
	}
}