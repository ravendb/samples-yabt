namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs;

public class UserStoryGetResponse : BacklogItemGetResponseBase
{
	public string? AcceptanceCriteria { get; set; }
}