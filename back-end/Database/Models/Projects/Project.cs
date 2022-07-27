using System;

namespace Raven.Yabt.Database.Models.Projects;

public class Project : BaseEntity, ITenant
{
	public string Name { get; set; } = null!;
	public string SourceUrl { get; set; } = null!;
	public DateTime LastUpdated { get; set; } = DateTime.MinValue;
}