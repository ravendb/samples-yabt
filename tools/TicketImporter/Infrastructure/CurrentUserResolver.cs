using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.TicketImporter.Infrastructure
{
	internal class CurrentUserResolver : ICurrentUserResolver
	{
		public string GetCurrentUserId() => "<IMPORTED>";
	}
}
