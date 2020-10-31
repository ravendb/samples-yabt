using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.BacklogItemServices.ByIdQuery;
using Raven.Yabt.Domain.BacklogItemServices.Commands;
using Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

using Xunit;

namespace Raven.Yabt.Domain.Tests.BacklogItemServices
{
	public class BacklogItemCreatedByLastUpdatedByTests : ConfigureTestEnvironment
	{
		private readonly IBacklogItemCommandService _commandService;
		private readonly IBacklogItemByIdQueryService _queryByIdService;
		private readonly IUserCommandService _userCmdService;
		private ICurrentUserResolver _currentUserResolver = null!;

		private string _currentUserId = null!;

		public BacklogItemCreatedByLastUpdatedByTests()
		{
			_commandService = Container.GetService<IBacklogItemCommandService>();
			_queryByIdService = Container.GetService<IBacklogItemByIdQueryService>();
			_userCmdService = Container.GetService<IUserCommandService>();
		}

		protected override void ConfigureIocContainer(IServiceCollection services)
		{
			base.ConfigureIocContainer(services);

			_currentUserResolver = Substitute.For<ICurrentUserResolver>();
			_currentUserResolver.GetCurrentUserId().Returns(c => _currentUserId);
			services.AddScoped(x => _currentUserResolver);
		}

		[Fact]
		private async Task Creating_BacklogItem_Sets_CreatedBy_And_LastUpdatedBy_Fields()
		{
			// GIVEN the current is user Homer Simpson
			var (homerId, _) = await SeedTwoUsers();
			_currentUserId = homerId;

			// WHEN creating a new backlog item
			var itemRef = await CreateBacklogItem();

			// THEN both its properties 'Created By' and 'Modified By' show Homer Simpson
			var item = (await _queryByIdService.GetById(itemRef.Id!)).Value;
			Assert.NotNull(item);
			Assert.NotNull(item.Created.ActionedBy);
			Assert.Equal(homerId, item.Created.ActionedBy.Id);
			Assert.NotNull(item.LastUpdated.ActionedBy);
			Assert.Equal(homerId, item.LastUpdated.ActionedBy.Id);
		}

		[Fact]
		private async Task Updating_BacklogItem_Sets_LastUpdatedBy_And_Doesnt_Change_CreatedBy()
		{
			// GIVEN a backlog item created by Homer Simpson
			var (homerId, margeId) = await SeedTwoUsers();
			_currentUserId = homerId;
			var itemRef = await CreateBacklogItem();

			// WHEN Marge Simpson updates the backlog item
			_currentUserId = margeId;
			await _commandService.Update(itemRef.Id!, new BugAddUpdRequest { Title = "Updated" });
			await SaveChanges();

			// THEN 
			var item = (await _queryByIdService.GetById(itemRef.Id!)).Value;
			//  its 'Created By' remains unchanged
			Assert.Equal(homerId, item.Created.ActionedBy.Id);
			// its 'Modified By' points to Marge Simpson
			Assert.Equal(margeId, item.LastUpdated.ActionedBy.Id);
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
			var homerId = (await _userCmdService.Create(dto)).Value.Id!;

			dto.FirstName = "Marge";
			var margeId = (await _userCmdService.Create(dto)).Value.Id!;

			await SaveChanges();

			return (homerId, margeId);
		}
	}
}
