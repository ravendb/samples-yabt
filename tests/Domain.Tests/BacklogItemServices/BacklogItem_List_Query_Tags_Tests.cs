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
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Query;

using Xunit;
// ReSharper disable UnusedVariable

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_List_Query_Tags_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemListQueryService _queryService;

	private readonly UserReference _currentUser = new UserReference { Id = "1-A", Name = "H. Simpson", FullName = "Homer Simpson" };

	public BacklogItem_List_Query_Tags_Tests()
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

	[Theory]
	[InlineData(null, 3, 3)]
	[InlineData("nonexisting", 0, null)]
	[InlineData("t", 3, null)]
	[InlineData("t1", 1, 3)]
	[InlineData("t2", 1, 3)]
	[InlineData("t3", 1, 1)]
	private async Task Querying_Tags_Works(string? search, int expectedRecordsCount, int? expectedCountOnFirstTag)
	{
		// GIVEN backlog items with various combination of 3 tags
		await CreateBacklogItem();
		await CreateBacklogItem("t1ag");
		await CreateBacklogItem("t2ag");
		await CreateBacklogItem("t1ag", "t2ag");
		await CreateBacklogItem("t2ag", "t1ag", "t3ag");

		// WHEN querying tags applying search (optional)
		var items = await _queryService.GetTags(new BacklogItemTagListGetRequest { Search = search });

		// THEN check
		// the number of returned tags
		Assert.Equal(expectedRecordsCount, items.Length);
		// number of tickets where the 1st tag is used
		if (expectedCountOnFirstTag != null)
			Assert.Equal(expectedCountOnFirstTag, items.First().Count);
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