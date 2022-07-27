using Raven.Yabt.Domain.Common;

namespace Raven.Yabt.Domain.UserServices.Query.DTOs;

public class UserListGetRequest : ListRequest<UsersOrderColumns>
{
	public string? Search { get; set; }
}