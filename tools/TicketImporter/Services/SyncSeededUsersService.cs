using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoBogus;

using Bogus;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;
using Raven.Yabt.Domain.UserServices.Query;
using Raven.Yabt.Domain.UserServices.Query.DTOs;
using Raven.Yabt.TicketImporter.Configuration;

namespace Raven.Yabt.TicketImporter.Services;

internal interface ISyncSeededUsersService
{
	Task<IList<UserReference>> GetGeneratedOrFetchedUsers();
}

internal class SyncSeededUsersService: ISyncSeededUsersService
{
	private readonly int _numberOfUsers;
	private readonly Dictionary<string,List<UserReference>> _userRefs = new ();
	private readonly IUserCommandService _userCmdService;
	private readonly IUserQueryService _userQueryService;
	private readonly ICurrentTenantResolver _tenantResolver;
	private readonly Faker<UserAddUpdRequest> _userFaker;

	public SyncSeededUsersService(GeneratedRecordsSettings settings, IUserCommandService userCmdService, IUserQueryService userQueryService, ICurrentTenantResolver tenantResolver)
	{
		_numberOfUsers = settings.NumberOfUsers;
		_userCmdService = userCmdService;
		_userQueryService = userQueryService;
		_tenantResolver = tenantResolver;

		_userFaker = new AutoFaker<UserAddUpdRequest>()
		             .RuleFor(fake => fake.AvatarUrl, _ => null)
		             .RuleFor(fake => fake.FirstName, fake => fake.Name.FirstName())
		             .RuleFor(fake => fake.LastName,  fake => fake.Name.LastName())
		             .RuleFor(fake => fake.Email,	 (_, p) => $"{p.FirstName}.{p.LastName}@yabt.ravendb.net");
	}

	public async Task<IList<UserReference>> GetGeneratedOrFetchedUsers()
	{
		var currentTenantId = _tenantResolver.GetCurrentTenantId();
		if (!_userRefs.ContainsKey(currentTenantId))
			_userRefs.Add(currentTenantId, new List<UserReference>());
			
		// Returned cached users if any
		if (_userRefs[currentTenantId].Any())
			return _userRefs[currentTenantId];
			
		// If don't need to generate, read from the DB
		var userList = await _userQueryService.GetList(new UserListGetRequest { PageSize = _numberOfUsers });
		if (userList.TotalRecords > 0)
		{
			_userRefs[currentTenantId]!.AddRange(
					userList.Entries.Select(u => new UserReference { Id = u.Id, Name = u.NameWithInitials, FullName = u.FullName })
				);
			return _userRefs[currentTenantId]!;
		}

		// Generate users
		for (var i=0; i < _numberOfUsers; i++)
		{
			var dto = _userFaker.Generate();
			var resp = await _userCmdService.Create(dto);
			if (!resp.IsSuccess)
				throw new Exception("Failed to create a new user");
				
			_userRefs[currentTenantId]!.Add(resp.Value);
		}
		return _userRefs[currentTenantId]!;
	}
}