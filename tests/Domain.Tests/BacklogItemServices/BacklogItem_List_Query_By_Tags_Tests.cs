using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;
// ReSharper disable UnusedVariable

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_List_Query_By_Tags_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemListQueryService _queryService;

	private readonly UserReference _currentUser = new UserReference { Id = "1-A", Name = "H. Simpson", FullName = "Homer Simpson" };

	public BacklogItem_List_Query_By_Tags_Tests()
	{
		_commandService = Container.GetService<IBacklogItemCommandService>()!;
		_queryService = Container.GetService<IBacklogItemListQueryService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		var currentUserResolver = Substitute.For<ICurrentUserResolver>();
		currentUserResolver.GetCurrentUserId().Returns(c => _currentUser.Id);
		services.AddScoped(x => currentUserResolver);
		var userResolver = Substitute.For<IUserReferenceResolver>();
		userResolver.GetCurrentUserReference().Returns(_currentUser);
		services.AddScoped(x => userResolver);
	}

	[Fact]
	private async Task Querying_By_Tag_Works()
	{
		// GIVEN 2 backlog items
		var itemRef1 = await CreateBacklogItem("tag1");
		var itemRef2 = await CreateBacklogItem("tag1", "tag2");

		// WHEN querying items by 'tag2'
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				Tags = new [] { "tag2" }
			});

		// THEN 
		// the returned only 1 record
		Assert.Equal(1, items.TotalRecords);
		// with correct ID
		Assert.Equal(itemRef2.Id, items.Entries.First().Id);
	}

	[Fact]
	private async Task Querying_By_2Tags_Works()
	{
		// GIVEN 4 backlog items
		var itemRef1 = await CreateBacklogItem("tag1");
		var itemRef2 = await CreateBacklogItem("tag2");
		var itemRef3 = await CreateBacklogItem("tag1", "tag2");
		var itemRef4 = await CreateBacklogItem("tag2", "tag1", "tag3");

		// WHEN querying items by 'tag1' and 'tag2'
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				Tags = new [] { "tag1", "tag2" },
				OrderBy = BacklogItemsOrderColumns.Number,
				OrderDirection = OrderDirections.Asc
			});

		// THEN 
		// the returned only 2 record
		Assert.Equal(2, items.TotalRecords);
		// with correct ID
		Assert.Equal(new [] { itemRef3.Id, itemRef4.Id }, items.Entries.Select(e => e.Id));
	}

	private async Task<BacklogItemReference> CreateBacklogItem(params string[] tags)
	{
		var dto = new BugAddUpdRequest { Title = "Test_" + GetRandomString(), Tags = tags };
		var addedRef = await _commandService.Create(dto);
		if (!addedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");
		await SaveChanges();

		return addedRef.Value;
	}
}