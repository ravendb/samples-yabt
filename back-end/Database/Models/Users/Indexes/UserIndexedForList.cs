namespace Raven.Yabt.Database.Models.Users.Indexes;

public class UserIndexedForList : User, ISearchable
{
	public string Search { get; set; } = null!;
}