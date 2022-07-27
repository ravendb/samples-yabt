using System.Linq;

using Raven.Client.Documents.Indexes;

namespace Raven.Yabt.Database.Models.Users.Indexes;

#pragma warning disable CS8602  // Suppress "Dereference of a possibly null reference", as Raven handles it on its own
// ReSharper disable once InconsistentNaming
public class Users_ForList : AbstractIndexCreationTask<User, UserIndexedForList>
{
	public Users_ForList()
	{
		// Add fields that are used for filtering and sorting
		Map = users =>
			from user in users
			select new
			{
				FullName = user.LastName + " " + user.FirstName,    // sort
				user.Email,											// sort
				user.RegistrationDate,								// sort
				user.TenantId,										// filter

				Search = new[] { user.FirstName.ToLower(), user.LastName.ToLower() },
			};

		Index(m => m.Search, FieldIndexing.Search);
		Analyzers.Add(x => x.Search, "WhitespaceAnalyzer");
	}
}
#pragma warning restore CS8602 // Dereference of a possibly null reference.