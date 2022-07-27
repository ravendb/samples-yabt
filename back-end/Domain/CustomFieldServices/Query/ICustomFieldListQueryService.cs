using System.Collections.Generic;
using System.Threading.Tasks;

using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

namespace Raven.Yabt.Domain.CustomFieldServices.Query;

public interface ICustomFieldListQueryService
{
	Task<ListResponse<CustomFieldListGetResponse>> GetList(CustomFieldListGetRequest dto);
		
	Task<CustomFieldListGetResponse[]> GetArray(CustomFieldListGetRequest dto);

	Task<IList<string>> VerifyExistingItems(IEnumerable<string> ids);
}