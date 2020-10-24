using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Database.Models.Users.Indexes;
using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.UserServices.Query
{
	public class MentionedUserResolver : BaseService<User>, IMentionedUserResolver
	{
		private readonly Regex _mentionRegex = new Regex(@"(?<=\B\@)([\w\._\-\/]+)", RegexOptions.Compiled);	// Get any word starting with '@'

		public MentionedUserResolver(IAsyncDocumentSession dbSession) : base(dbSession) {}

		/// <summary>
		/// 	Resolves mentioned users from a text with references
		/// </summary>
		/// <param name="text"> A human-readable text (e.g. "@HomerSimpson asked to mary @MargeSimpson") </param>
		/// <returns>
		/// 	List of mentioned users with their references in the text (e.g. { 'MargeSimpson', 'user/1-A' })
		/// </returns>
		public Task<IDictionary<string, string>> GetMentionedUsers(string text)
		{
			var userReferences = GetUserReferencesFromText(text);
			return GetResolvedMentionedUsers(userReferences);
		}

		public async Task<(string, IDictionary<string, string>)> GetUpdatedReferencesOfMentionedUsers(string commentMessage, IDictionary<string, string> currentMentionedUserIds)
		{
			var newMentionedUserIds = await GetResolvedMentionedUsers(currentMentionedUserIds.Values);

			commentMessage = _mentionRegex.Replace(
				commentMessage,
				match => currentMentionedUserIds.TryGetValue(match.Groups[1].Value, out var val) 
					? newMentionedUserIds.SingleOrDefault(m => m.Value == val).Key  
					: match.Groups[1].Value);
			
			return (commentMessage, newMentionedUserIds);
		}

		/// <summary>
		/// 	Get mentions of users from the text
		/// </summary>
		private string[] GetUserReferencesFromText(string text)
		{
			var matches = _mentionRegex.Matches(text);
			return matches.Distinct().Select(m => m.Value).ToArray();
		}

		/// <summary>
		/// 	Resolve all the mentioned names with user's IDs from the DB
		/// </summary>
		/// <returns>
		/// 	List of mentioned users with their references in the text (e.g. { 'MargeSimpson', 'user/1-A' })
		/// </returns>
		private async Task<IDictionary<string, string>> GetResolvedMentionedUsers(ICollection<string> userReferences)
		{
			if (!userReferences.Any())
				return new Dictionary<string, string>();

			var query = from user in DbSession.Query<MentionedUsersIndexed, Users_Mentions>()
				where user.MentionedName.In(userReferences)
				select user;
			var users = await query.As<User>().ToArrayAsync();
			
			return users.ToDictionary(x=>x.MentionedName, x=>x.Id);
		}
	}
}
