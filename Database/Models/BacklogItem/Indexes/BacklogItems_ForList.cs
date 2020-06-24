using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Raven.Client.Documents.Indexes;
using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Database.Models.BacklogItem.Indexes
{
	[SuppressMessage("Compiler", "CS8602")] // Suppress "Dereference of a possibly null reference", as Raven handles it on its own
	public class BacklogItems_ForList : AbstractIndexCreationTask<BacklogItem, BacklogItemIndexedForList>
	{
		public BacklogItems_ForList()
		{
			// Add fields that are used for filtering and sorting
			Map = tickets =>
				from ticket in tickets
					let created		= ticket.ModifiedBy.OrderBy(t => t.Timestamp).First()
					let lastUpdated	= ticket.ModifiedBy.OrderBy(t => t.Timestamp).Last()
				select new
				{
					ticket.Title,       // sort
					ticket.Type,        // filter
					AssignedUserId = ticket.Assignee.Id,    // filter

					CreatedByUserId = created.ActionedBy.Id,		// filter
					CreatedTimestamp = created.Timestamp,			// sort
					LastUpdatedTimestamp = lastUpdated.Timestamp,	// sort

					((BacklogItemBug)ticket).Severity,  // sort		Note that 'ticket as BacklogItemBug' would cause a runtime error on building the index
					((BacklogItemBug)ticket).Priority,  // sort

					Search = new[] {
								ticket.Title,
								((BacklogItemBug)ticket).StepsToReproduce,
								((BacklogItemUserStory)ticket).AcceptanceCriteria
							}
							.Concat(ticket.Comments.Select(c => c.Message)),

					// Dynamic fields
					// Notes:
					//	- The format 'collection_key' is required to treat them as dictionary in the C# code
					//	- Prefix is vital, see https://groups.google.com/d/msg/ravendb/YvPZFIn5GVg/907Msqv4CQAJ

					// Create a dictionary for Modifications
					_ = ticket.ModifiedBy.GroupBy(m => m.ActionedBy.Id)                                                           // filter & sort by Timestamp
											.Select(x => CreateField($"{nameof(BacklogItemIndexedForList.ModifiedByUser)}_{x.Key!.Replace("/","").ToLower()}", 
																	 x.Max(o => o.Timestamp)
																	 )
													),
					// Create a dictionary for Custom Fields
					__ = from x in ticket.CustomFields
						 let fieldType = LoadDocument<CustomField.CustomField>(x.Key).FieldType
						 select 
							(fieldType == CustomFieldType.Text)
								? CreateField($"{nameof(BacklogItem.CustomFields)}_{x.Key.Replace("/","").ToLower()}", x.Value, false, true)	// search in text Custom Fields
								: CreateField($"{nameof(BacklogItem.CustomFields)}_{x.Key.Replace("/","").ToLower()}", x.Value)					// filter by other Custom Fields (exact match)
				};

			Index(m => m.Search, FieldIndexing.Search);
			Analyzers.Add(x => x.Search, "StandardAnalyzer");
		}
	}
}
