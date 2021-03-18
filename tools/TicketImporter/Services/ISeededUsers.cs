using System.Collections.Generic;
using System.Threading.Tasks;

using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.TicketImporter.Services
{
	internal interface ISeededUsers
	{
		Task<IList<UserReference>> GetGeneratedOrFetchedUsers();
	}
}