namespace Raven.Yabt.Database.Models;

public abstract class BaseEntity : IEntity
{
	/// <summary>
	///		The record ID
	/// </summary>
	/// <remarks>
	///		Set by Raven Client. Can be temporarily null before passed to the DocumentSession.Store() method
	/// </remarks>
	public string Id { get; protected set; } = null!;
}