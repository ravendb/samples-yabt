#nullable disable  // Disable nullable check for a response DTO file
namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs;

public class LabelResponse
{
	public uint Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string Color { get; set; }
}