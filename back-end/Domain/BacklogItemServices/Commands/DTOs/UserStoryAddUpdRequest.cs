namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

public record UserStoryAddUpdRequest : BacklogItemAddUpdRequestBase
{
	public string? AcceptanceCriteria { get; set; }
}