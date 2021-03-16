using System.Linq;

using Raven.Client.Documents.Indexes;

namespace Raven.Yabt.Database.Models.BacklogItems.Indexes
{
	// ReSharper disable once InconsistentNaming
	public class BacklogItems_Tags : AbstractIndexCreationTask<BacklogItem, BacklogItemTagsIndexed>
	{
		public BacklogItems_Tags()
		{
			/*
			 * Using Count() in Map/Reduce is prohibited, using Sum() is the advised workaround 
			 * See more at https://stackoverflow.com/a/14236661/968003
			 */
			Map = tickets => 
				from t in tickets
				from tag in t.Tags
				where tag != null
				select new BacklogItemTagsIndexed
				{
					Name = tag.ToLower(),
					Count = 1
				};
			Reduce = results => from r in results
				group r by r.Name into g
				select new BacklogItemTagsIndexed
				{
					Name = g.Key,
					Count = g.Sum(i => i.Count)
				};
			
			StoreAllFields(FieldStorage.Yes);
		}
	}
}
