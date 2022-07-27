using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class BacklogItem_List_Query_By_CurrentUser_Tests : ConfigureTestEnvironment
{
	private readonly IBacklogItemCommandService _commandService;
	private readonly IBacklogItemListQueryService _queryService;
	private readonly IUserCommandService _userCmdService;
	private ICurrentUserResolver _currentUserResolver = null!;

	private string _currentUserId = null!;

	public BacklogItem_List_Query_By_CurrentUser_Tests()
	{
		_commandService = Container.GetService<IBacklogItemCommandService>()!;
		_queryService = Container.GetService<IBacklogItemListQueryService>()!;
		_userCmdService = Container.GetService<IUserCommandService>()!;
	}

	protected override void ConfigureIocContainer(IServiceCollection services)
	{
		base.ConfigureIocContainer(services);

		_currentUserResolver = Substitute.For<ICurrentUserResolver>();
		_currentUserResolver.GetCurrentUserId().Returns(_ => _currentUserId);
		services.AddScoped(_ => _currentUserResolver);
	}

	[Fact]
	private async Task Querying_By_Created_User_Works()
	{
		// GIVEN 2 users (current and another one)
		var (homerId, margeId) = await SeedTwoUsers();
		_currentUserId = homerId;
		//	and 2 backlog items created by the 'current' user
		await CreateBacklogItem();
		await CreateBacklogItem();
		//	and 1 by 'another' user
		_currentUserId = margeId;
		var anotherRef = await CreateBacklogItem();
		_currentUserId = homerId;

		// WHEN querying items created by another user
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				UserModification = new BacklogItemModification
				{
					UserId = margeId,
					Type = BacklogItemModification.ModificationType.CreatedOnly
				}
			});

		// THEN 
		// the returned only 1 record created by 'another' user
		Assert.Equal(1, items.TotalRecords);
		Assert.Equal(margeId, items.Entries[0].Created.ActionedBy.Id);
		// with correct backlog ID
		Assert.Equal(anotherRef.Id, items.Entries[0].Id);
	}

	[Fact]
	private async Task Querying_On_Created_Or_Modified_By_User_Works()
	{
		// GIVEN 2 users (current and another one)
		var (homerId, margeId) = await SeedTwoUsers();
		_currentUserId = homerId;
		//	and 2 backlog items created by the 'current' user
		await CreateBacklogItem();
		var modifRef = await CreateBacklogItem();
		//	with 1 modified by 'another' user
		_currentUserId = margeId;
		await _commandService.AssignToUser(modifRef.Id!, margeId);
		await SaveChanges();
		_currentUserId = homerId;

		// WHEN querying items created by another user
		var items = await _queryService.GetList(
			new BacklogItemListGetRequest
			{
				UserModification = new BacklogItemModification
				{
					UserId = margeId,
					Type = BacklogItemModification.ModificationType.Any
				}
			});

		// THEN 
		// only 1 record modified by 'another' user is returned
		Assert.Equal(1, items.TotalRecords);
		Assert.Equal(margeId, items.Entries[0].LastUpdated.ActionedBy.Id);
		// with correct backlog ID
		Assert.Equal(modifRef.Id, items.Entries[0].Id);
	}

	[Fact]
	private async Task Querying_On_Created_By_Me_Works()
	{
		// GIVEN 2 users
		var (homerId, margeId) = await SeedTwoUsers();
		_currentUserId = homerId;
		//	and 2 backlog items created by Homer
		await CreateBacklogItem();
		await CreateBacklogItem();
		//	with 1 created by Marge
		_currentUserId = margeId;
		await CreateBacklogItem();
			
		// WHEN the current user is Homer 
		//		and querying items created/modified by him
		_currentUserId = homerId;
		var homersItems = await _queryService.GetList( new BacklogItemListGetRequest { CurrentUserRelation = CurrentUserRelations.ModifiedBy } );

		// THEN 
		//  only 2 record created by Homer are returned
		Assert.Equal(2, homersItems.TotalRecords);
		Assert.Equal(homerId, homersItems.Entries[0].Created.ActionedBy.Id);
		Assert.Equal(homerId, homersItems.Entries[1].Created.ActionedBy.Id);
	}

	[Fact]
	private async Task Querying_On_Created_Or_Modified_By_Me_Takes_Into_Account_My_Updates()
	{
		// GIVEN 2 users
		var (homerId, margeId) = await SeedTwoUsers();
		_currentUserId = homerId;
		//	and 2 backlog items created by Homer
		await CreateBacklogItem();
		var modifRef = await CreateBacklogItem();
		//	with 1 created by Marge
		_currentUserId = margeId;
		var createdRef = await CreateBacklogItem();
		// and 1 modified by Marge
		await _commandService.AssignToUser(modifRef.Id!, margeId);
		await SaveChanges();
			
		// WHEN the current user is Marge 
		//		and querying items created/modified by him
		var margesItems = await _queryService.GetList(new BacklogItemListGetRequest { CurrentUserRelation = CurrentUserRelations.ModifiedBy });

		// THEN 
		// only 2 record created/modified by Marge are returned
		Assert.Equal(2, margesItems.TotalRecords);
		Assert.Contains(margesItems.Entries, i => i.Id == createdRef.Id);
		Assert.Contains(margesItems.Entries, i => i.Id == modifRef.Id);
	}

	private async Task<BacklogItemReference> CreateBacklogItem()
	{
		var dto = new BugAddUpdRequest { Title = "Test_" + GetRandomString() };
		var addedRef = await _commandService.Create(dto);
		if (!addedRef.IsSuccess)
			throw new Exception("Failed to create a backlog item");
		await SaveChanges();

		return addedRef.Value;
	}

	private async Task<(string, string)> SeedTwoUsers()
	{
		var dto = new UserAddUpdRequest { FirstName = "Homer", LastName = "Simpson" };
		var homer = await _userCmdService.Create(dto);
		if (!homer.IsSuccess)
			throw new Exception("Failed to create a user for Homer");

		dto.FirstName = "Marge";
		var marge = await _userCmdService.Create(dto);
		if (!marge.IsSuccess)
			throw new Exception("Failed to create a user for Marge");

		await SaveChanges();

		return (homer.Value.Id!, marge.Value.Id!);
	}
}