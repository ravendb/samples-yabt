namespace Raven.Yabt.Domain.BacklogItemServices.ByIdQuery.DTOs
{
	public class UserStoryGetResponse : BacklogItemGetResponse
	{
		public string? AcceptanceCriteria { get; set; }
	}
}
