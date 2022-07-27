using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Database.Models.Users.Indexes;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.Helpers;

namespace Raven.Yabt.Domain.UserServices.Query;

public class MentionedUserResolver : BaseService<User>, IMentionedUserResolver
{
	/// <summary>
	/// 	Gets any word starting with '@'
	/// </summary>
	private readonly Regex _mentionRegex = new Regex(@"(?<=\B\@)([\w\._\-\/]+)", RegexOptions.Compiled);
		
	public MentionedUserResolver(IAsyncTenantedDocumentSession dbSession) : base(dbSession) {}

	/// <summary>
	/// 	Resolves mentioned users from a text with references
	/// </summary>
	/// <param name="text"> A human-readable text (e.g. "@HomerSimpson asked to mary @MargeSimpson") </param>
	/// <returns>
	/// 	List of mentioned users with their references in the text (e.g. { 'MargeSimpson', '1-A' })
	/// </returns>
	public async Task<IDictionary<string, string>> GetMentionedUsers(string text)
	{
		// Get mentions of users from the text
		var matches = _mentionRegex.Matches(text);
		var userReferences = matches.Distinct().Select(m => m.Value).ToArray();
			
		// Resolve all the mentioned names with user's IDs from the DB
		if (userReferences.Any() != true)
			return new Dictionary<string, string>();

		var query = from user in DbSession.Query<MentionedUsersIndexed, Users_Mentions>()
			where user.MentionedName.In(userReferences)
			select user;
		var users = (await query.As<User>().ToArrayAsync())
		            .Select(u => u.ToReference().RemoveEntityPrefixFromId())
		            .ToList();
		return users.ToDictionary(x=>x.MentionedName, x=>x.Id!);
	}
}