using System.Linq;

using Raven.Client.Documents.Indexes;

namespace Raven.Yabt.Database.Models.Users.Indexes;

// ReSharper disable once InconsistentNaming
public class Users_Mentions : AbstractIndexCreationTask<User, MentionedUsersIndexed>
{
	public Users_Mentions()
	{
		Map = users =>
			from user in users
			select new MentionedUsersIndexed
			{
				MentionedName = user.FirstName + user.LastName,	// filter,
				TenantId = user.TenantId						// filter
			};
	}
}