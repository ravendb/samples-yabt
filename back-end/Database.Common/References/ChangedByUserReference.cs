using System;

namespace Raven.Yabt.Database.Common.References;

public record ChangedByUserReference
{
	private readonly DateTime _timestamp = DateTime.UtcNow;

	/// <summary>
	///		Timestamp of the change
	/// </summary>
	public DateTime Timestamp
	{
		get => _timestamp; 
		init => _timestamp = value;
	}

	/// <summary>
	///		The user who made the change
	/// </summary>
	public UserReference ActionedBy { get; set; } = null!;
		
	public ChangedByUserReference() {}

	public ChangedByUserReference(UserReference actionedBy)
	{
		ActionedBy = actionedBy;
	}
	public ChangedByUserReference(UserReference actionedBy, DateTime timestamp) : this(actionedBy)
	{
		Timestamp = timestamp;
	}

	public ChangedByUserReference GetCopy()
	{
		return new(ActionedBy with { }, Timestamp);
	}
}