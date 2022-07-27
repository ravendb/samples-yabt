using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Raven.Client.Documents.Indexes;

namespace Raven.Yabt.Database.Models.CustomFields.Indexes;

[SuppressMessage("Compiler", "CS8602")] // Suppress "Dereference of a possibly null reference", as Raven handles it on its own
// ReSharper disable once InconsistentNaming
public class CustomFields_ForList : AbstractIndexCreationTask<CustomField, CustomFieldIndexedForList>
{
	public CustomFields_ForList()
	{
		// Add fields that are used for filtering and sorting
		Map = fields =>
			from field in fields
			select new CustomFieldIndexedForList
			{
				TenantId		= field.TenantId,			// filter
				Name			= field.Name,				// sort & filter
				BacklogItemTypes= field.BacklogItemTypes	// filter
			};
	}
}