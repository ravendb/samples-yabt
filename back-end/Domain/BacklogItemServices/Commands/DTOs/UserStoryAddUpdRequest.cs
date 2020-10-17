namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs
{
	public class UserStoryAddUpdRequest : BacklogItemAddUpdRequestBase
	{
		public string? AcceptanceCriteria { get; set; }
	}
}
