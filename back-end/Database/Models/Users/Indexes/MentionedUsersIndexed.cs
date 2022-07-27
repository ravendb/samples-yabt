namespace Raven.Yabt.Database.Models.Users.Indexes;

public class MentionedUsersIndexed : User
{
	public string? MentionedName { get; set; }
}