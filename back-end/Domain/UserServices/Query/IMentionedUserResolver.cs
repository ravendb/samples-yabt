using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raven.Yabt.Domain.UserServices.Query;

public interface IMentionedUserResolver
{
	Task<IDictionary<string, string>> GetMentionedUsers(string text);
}