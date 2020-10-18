using System;
using System.Linq;

using Raven.Yabt.Domain.Infrastructure;
using Raven.Yabt.TicketImporter.Services;

namespace Raven.Yabt.TicketImporter.Infrastructure
{
	/// <summary>
	///		Get a random user as the current one
	/// </summary>
	internal class CurrentUserResolver : ICurrentUserResolver
	{
		private readonly ISeededUsers _seededUser;

		public CurrentUserResolver(ISeededUsers seededUser)
		{
			_seededUser = seededUser;
		}

		/// <summary>
		///		Get a random generated user as the current one
		/// </summary>
		public string GetCurrentUserId() => _seededUser.GetGeneratedUsers().Result.OrderBy(x => Guid.NewGuid()).First();
	}
}
