using System;
using System.Collections.Generic;
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
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_Update_RelatedItems_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemByIdQueryService _queryService;

	private readonly UserReference _currentUser = new (){ Id = "1", Name = "H. Simpson", FullName = "Homer Simpson" };

	public BacklogItem_Update_RelatedItems_Tests()
	{
		_commandService = Container.GetService<IBacklogItemCommandService>()!;
		_queryService = Container.GetService<IBacklogItemByIdQueryService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		var userResolver = Substitute.For<IUserReferenceResolver>();
		userResolver.GetCurrentUserReference().Returns(_currentUser);
		services.AddScoped(_ => userResolver);
	}

	[Fact]
	private async Task Added_Related_Item_Get_Persisted_In_Both_Tickets()
	{
		// GIVEN 2 tickets
		var (mainTicketId, _) = await CreateSampleBug();
		var (refTicketId, _) = await CreateSampleBug();

		// WHEN add a reference to another ticket
		var dto = GetAddUpdateDto(d => 
			d.ChangedRelatedItems = new List<BacklogRelationshipAction>
			{
				new()
				{
					BacklogItemId = refTicketId!,
					RelationType = BacklogRelationshipType.Related,
					ActionType = ListActionType.Add
				}
			});
		await _commandService.Update(mainTicketId!, dto);
		await SaveChanges();
			
		// THEN 
		// the new ticket has corrected 'Related Items'
		var (mainTicket, _) = await _queryService.GetById(mainTicketId!);
		Assert.True(mainTicket.RelatedItems?.Count == 1);
			
		var relItem = mainTicket.RelatedItems!.Single();
		Assert.Equal(BacklogRelationshipType.Related, relItem.LinkType);
		Assert.Equal(refTicketId!, relItem.RelatedTo.Id);
			
		// and the the referred ticket is mirroring 'Related Items' too
		var (refTicket, _) = await _queryService.GetById(refTicketId!);
		Assert.True(refTicket.RelatedItems?.Count == 1);
			
		relItem = refTicket.RelatedItems!.Single();
		Assert.Equal(BacklogRelationshipType.Related, relItem.LinkType);
		Assert.Equal(mainTicketId!, relItem.RelatedTo.Id);
	}

	[Theory]
	[InlineData(BacklogRelationshipType.Blocks, BacklogRelationshipType.BlockedBy)]
	[InlineData(BacklogRelationshipType.BlockedBy, BacklogRelationshipType.Blocks)]
	[InlineData(BacklogRelationshipType.CausedBy, BacklogRelationshipType.Causes)]
	[InlineData(BacklogRelationshipType.Related, BacklogRelationshipType.Related)]
	[InlineData(BacklogRelationshipType.Duplicate, BacklogRelationshipType.Duplicate)]
	private async Task Adding_Related_Items_Mirrors_On_Other_Tickets_Correctly(BacklogRelationshipType setLinkType, BacklogRelationshipType expectedMirroringLinkType)
	{
		// GIVEN 2 tickets
		var (refTicketId, _) = await CreateSampleBug();
		// where one refers to another
		await CreateSampleBug(t => t.ChangedRelatedItems = new List<BacklogRelationshipAction>
				{
					new ()
					{
						BacklogItemId = refTicketId!, 
						RelationType = setLinkType, 
						ActionType = ListActionType.Add
					}
				}
			);

		// THEN 
		// the 'Related Item' on the referenced ticket has correct link type
		var (refTicket, _) = await _queryService.GetById(refTicketId!);
		Assert.Equal(expectedMirroringLinkType, refTicket.RelatedItems!.Single().LinkType);
	}

	[Fact]
	private async Task Deleted_Related_Item_Get_Reflected_In_Both_Tickets()
	{
		// GIVEN 2 tickets
		var (refTicketId, _) = await CreateSampleBug();
		// one of them has a related item
		var (mainTicketId, _) = await CreateSampleBug(GetRelatedItemAction(refTicketId!, ListActionType.Add));
			
		// When the related item gets removed
		var dto = GetAddUpdateDto(GetRelatedItemAction(refTicketId!, ListActionType.Remove));
		await _commandService.Update(mainTicketId!, dto);
		await SaveChanges();
			
		// THEN 
		// the main ticket's 'Related Item' are empty
		var (mainTicket, _) = await _queryService.GetById(mainTicketId!);
		Assert.True(mainTicket.RelatedItems?.Any() != true);
		// and the the referred ticket got the 'Related Item' deleted too
		var (refTicket, _) = await _queryService.GetById(refTicketId!);
		Assert.True(refTicket.RelatedItems?.Any() != true);
	}

	[Fact]
	private async Task Deleting_A_Ticket_Deletes_Its_References_In_Related_Items_Of_Other_Tickets()
	{
		// GIVEN 3 tickets
		var (mainTicketId, _) = await CreateSampleBug();
		// where the 'main' ticket is related to 2 others 
		var (refTicket1Id, _) = await CreateSampleBug(GetRelatedItemAction(mainTicketId!, ListActionType.Add));
		var (refTicket2Id, _) = await CreateSampleBug(GetRelatedItemAction(mainTicketId!, ListActionType.Add));
			
		// When the 'main' ticket is deleted
		await _commandService.Delete(mainTicketId!);
		await SaveChanges();
			
		// THEN 
		// the 'Related Item' of the 2 other tickets are empty
		var (refTicket1, _) = await _queryService.GetById(refTicket1Id!);
		Assert.True(refTicket1.RelatedItems?.Any() != true);
		var (refTicket2, _) = await _queryService.GetById(refTicket2Id!);
		Assert.True(refTicket2.RelatedItems?.Any() != true);
	}

	private async Task<BacklogItemReference> CreateSampleBug(Action<BugAddUpdRequest>? action = null)
	{
		var ticketAddedRef = await _commandService.Create(GetAddUpdateDto(action));
		if (!ticketAddedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");
		await SaveChanges();

		return ticketAddedRef.Value;
	}

	private static BugAddUpdRequest GetAddUpdateDto(Action<BugAddUpdRequest>? action = null)
	{
		var dto = new BugAddUpdRequest
		{
			Title = "Test Bug " + GetRandomString(),
			Severity = BugSeverity.Critical,
			Priority = BugPriority.P1
		};
		action?.Invoke(dto);
		return dto;
	}

	private static Action<BugAddUpdRequest> GetRelatedItemAction(string id, ListActionType actionType)
	{
		return d => d.ChangedRelatedItems =
			new List<BacklogRelationshipAction>
			{
				new()
				{
					BacklogItemId = id,
					RelationType = BacklogRelationshipType.Blocks,
					ActionType = actionType
				}
			};
	} 
}