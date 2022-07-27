using System.Collections.Generic;

using Raven.Yabt.Database.Common.BacklogItem;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;

public class CustomFieldListGetRequest : ListRequest<CustomFieldOrderColumns>
{
	public IEnumerable<string>? Ids { get; set; }
		
	public BacklogItemType? BacklogItemType { get; set; }
}