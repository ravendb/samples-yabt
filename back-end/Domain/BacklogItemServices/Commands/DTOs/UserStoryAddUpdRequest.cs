namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs
{
	public class UserStoryAddUpdRequest : BacklogItemAddUpdRequest
	{
		public string? AcceptanceCriteria { get; set; }
	}
}
