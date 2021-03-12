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
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices
{
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
		private async Task Added_Related_Item_Get_Persisted()
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
						ActionType = BacklogRelationshipActionType.Add
					}
				});
			await _commandService.Update(mainTicketId!, dto);
			await SaveChanges();
			
			// THEN 
			// the new ticket's properties appear in the DB
			var ticket = await _queryService.GetById(mainTicketId!);
			Assert.True(ticket.IsSuccess);
			Assert.True(ticket.Value.RelatedItems?.Count == 1);
			var relItem = ticket.Value.RelatedItems!.Single();
			Assert.Equal(BacklogRelationshipType.Related, relItem.LinkType);
			Assert.Equal(refTicketId!, relItem.RelatedTo.Id);
		}

		[Fact]
		private async Task Deleted_Related_Item_Get_Persisted()
		{
			// GIVEN 2 tickets
			var (refTicketId, _) = await CreateSampleBug();
			// one of them has a related item
			var (mainTicketId, _) = await CreateSampleBug(GetRelatedItemAction(refTicketId!, BacklogRelationshipActionType.Add));
			
			// When the related item gets removed
			var dto = GetAddUpdateDto(GetRelatedItemAction(refTicketId!, BacklogRelationshipActionType.Remove));
			await _commandService.Update(mainTicketId!, dto);
			await SaveChanges();
			
			// THEN 
			// the new ticket's properties appear in the DB
			var ticket = await _queryService.GetById(mainTicketId!);
			Assert.True(ticket.IsSuccess);
			Assert.True(ticket.Value.RelatedItems?.Any() != true);
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

		private static Action<BugAddUpdRequest> GetRelatedItemAction(string id, BacklogRelationshipActionType actionType)
		{
			return d => d.ChangedRelatedItems =
				new List<BacklogRelationshipAction>
				{
					new()
					{
						BacklogItemId = id,
						RelationType = BacklogRelationshipType.Related,
						ActionType = actionType
					}
				};
		} 
	}
}
