using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using AutoBogus;

using Bogus;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Domain.UserServices.Command;
using Raven.Yabt.Domain.UserServices.Command.DTOs;

namespace Raven.Yabt.TicketImporter.Services
{
	internal class SeededUsers: ISeededUsers
	{
		private const int UserQuantity = 100;

		private readonly List<UserReference> _userRefs = new List<UserReference>();
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

		public async Task<IList<UserReference>> GetGeneratedUsers()
		{
			if (_userRefs.Count > 0)
				return _userRefs;

			for (var i=0; i < UserQuantity; i++)
			{
				var dto = _userFaker.Generate();
				var resp = await _userService.Create(dto);
				if (!resp.IsSuccess)
					throw new Exception("Failed to create a new user");
				
				_userRefs!.Add(resp.Value);
			}
			return _userRefs!;
		}
	}
}
