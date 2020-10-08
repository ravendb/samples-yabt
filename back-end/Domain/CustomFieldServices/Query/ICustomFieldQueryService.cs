using System.Collections.Generic;
using System.Threading.Tasks;

using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Query
{
	public interface ICustomFieldQueryService
	{
		Task<CustomFieldListGetResponse[]> GetArray(CustomFieldListGetRequest dto);

		Task<IDictionary<string, string>> GetFullIdsOfExistingItems(IEnumerable<string> ids);
	}
}
