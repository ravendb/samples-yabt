using System.Linq;

using Raven.Client.Documents.Indexes;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Models.CustomFields;

namespace Raven.Yabt.Database.Models.BacklogItems.Indexes;

#pragma warning disable CS8602  // Suppress "Dereference of a possibly null reference", as Raven handles it on its own
// ReSharper disable once InconsistentNaming
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
				ticket.TenantId,								// filter
				ticket.Title,									// sort
				ticket.Type,									// filter
				AssignedUserId = ticket.Assignee.Id,			// filter
				AssignedUserName = ticket.Assignee.FullName,	// sort

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

				Tags = ticket.Tags!.Distinct().ToList(),	// filter (the RavenDB engine will gracefully handle if no tags)
				ticket.State,								// filter

				// Dynamic fields
				// Notes:
				//	- The format 'collection_key' is required to treat them as dictionary in the C# code
				//	- Prefix is vital, see https://groups.google.com/d/msg/ravendb/YvPZFIn5GVg/907Msqv4CQAJ

				// Create a dictionary for Modifications
				_ = ticket.ModifiedBy.GroupBy(m => m.ActionedBy.Id)															// filter & sort by Timestamp
				          .Select(x => CreateField($"{nameof(BacklogItemIndexedForList.ModifiedByUser)}_{x.Key.ToUpper()}", 
							          x.Max(o => o.Timestamp)
						          )
					          ),
				// Create a dictionary for mentioned users
				_1 = from um in 
						from comment in ticket.Comments 
						from user in comment.MentionedUserIds!
						select new { user, comment.LastModified }
					group um by um.user into g
					select CreateField(
							$"{nameof(BacklogItemIndexedForList.MentionedUser)}_{g.Key.Value.ToUpper()}",
							g.Max(f => f.LastModified)
						),
				// Create a dictionary for Custom Fields
				_2 = from x in ticket.CustomFields
					let fieldType = LoadDocument<CustomField>($"{nameof(CustomField)}s/{x.Key}").FieldType
					let key = $"{nameof(BacklogItem.CustomFields)}_{x.Key.ToUpper()}"
					select 
						fieldType == CustomFieldType.Text
							? CreateField(key, x.Value, false, true)	// search in text Custom Fields
							: CreateField(key, x.Value)					// filter by other Custom Fields (exact match)
			};

		Index(m => m.Search, FieldIndexing.Search);
		Analyzers.Add(x => x.Search, "StandardAnalyzer");
	}
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.