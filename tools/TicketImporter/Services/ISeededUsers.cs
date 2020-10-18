using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raven.Yabt.TicketImporter.Services
{
	internal interface ISeededUsers
	{
		Task<IList<string>> GetGeneratedUsers();
	}
}