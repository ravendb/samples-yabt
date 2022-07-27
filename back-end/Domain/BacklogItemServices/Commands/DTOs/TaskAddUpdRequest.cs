namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

public record TaskAddUpdRequest : BacklogItemAddUpdRequestBase
{
	public string? Description { get; set; }
}