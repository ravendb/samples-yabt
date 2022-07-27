namespace Raven.Yabt.Domain.BacklogItemServices.Commands.DTOs;

public record FeatureAddUpdRequest : BacklogItemAddUpdRequestBase
{
	public string? Description { get; set; }
}