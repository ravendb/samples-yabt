using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using DomainResults.Common;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_Tags_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemByIdQueryService _queryService;

	private readonly UserReference _currentUser = new (){ Id = "1", Name = "H. Simpson", FullName = "Homer Simpson" };

	public BacklogItem_Tags_Tests()
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
	private async Task Added_Tags_On_Creating_Backlog_Item_Get_Persisted()
	{
		// GIVEN a ticket with 2 tags
		var tags = new[] { "bla1", "bla2" };
		var (ticketRef, _) = await CreateSampleBug(d => d.Tags = tags);
			
		// THEN 
		// the tags are persisted for the ticket
		var (ticket, _) = await _queryService.GetById(ticketRef.Id!);
		Assert.Equal(tags, ticket.Tags);
	}

	[Fact]
	private async Task Tags_Get_Overwritten_On_Updating_Backlog_Item_Get_Persisted()
	{
		// GIVEN a ticket with 2 tags
		var (ticketRef, _) = await CreateSampleBug(d => d.Tags = new[] { "bla1", "bla2" });
			
		// WHEN replace them with other tags on update
		var tags = new[] { "foo1", "foo2" };
		var dto = GetAddUpdateDto(d => d.Tags = tags);
		await _commandService.Update(ticketRef.Id!, dto);
		await SaveChanges();
			
		// THEN 
		// the new tags get persisted instead of the old ones
		var (ticket, _) = await _queryService.GetById(ticketRef.Id!);
		Assert.Equal(tags, ticket.Tags);
	}

	[Theory]
	[InlineData(new[] { "123", "1234" }, true)]
	[InlineData(new[] { "1234567890", "123456789012" }, false)]
	[InlineData(new[] { "123456789012", "123456789012" }, false)]
	private async Task Tags_Longer_10_Symbols_Prohibited(string[] tags, bool isValid)
	{
		// GIVEN a ticket tags
		var (_, status) = await CreateSampleBug(d => d.Tags = tags);
			
		// THEN requests with long tags get denied
		Assert.Equal(isValid, status.IsSuccess);
	}

	private async Task<IDomainResult<BacklogItemReference>> CreateSampleBug(Action<BugAddUpdRequest>? action = null)
	{
		var ticketAddedRef = await _commandService.Create(GetAddUpdateDto(action));
		if (ticketAddedRef.IsSuccess)
			await SaveChanges();

		return ticketAddedRef;
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
}