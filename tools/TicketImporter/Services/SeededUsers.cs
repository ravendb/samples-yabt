using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoBogus;

using Bogus;

using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class SeededUsers: ISeededUsers
	{
		private const int USER_QUANTITY = 100;

		private readonly List<string> _userIds = new List<string>();
		private readonly IUserCommandService _userService;
		private readonly Faker<UserAddUpdRequest> _userFaker;

		public SeededUsers(IUserCommandService userService)
		{
			_userService = userService;

			_userFaker = new AutoFaker<UserAddUpdRequest>()
								.RuleFor(fake => fake.AvatarUrl, fake => null)
								.RuleFor(fake => fake.FirstName, fake => fake.Name.FirstName())
								.RuleFor(fake => fake.LastName,  fake => fake.Name.LastName())
								.RuleFor(fake => fake.Email,	 (_, p) => $"{p.FirstName}.{p.LastName}@yabt.com");
		}

		public async Task<IList<string>> GetGeneratedUsers()
		{
			if (_userIds?.Count > 0)
				return _userIds;

			for (var i=0; i < USER_QUANTITY; i++)
			{
				var dto = _userFaker.Generate();
				var resp = await _userService.Create(dto);
				if (!resp.IsSuccess)
					throw new Exception("Failed to create a new user");
				
				_userIds!.Add(resp.Value.Id!);
			}
			return _userIds!;
		}
	}
}
