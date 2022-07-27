// ReSharper disable ClassNeverInstantiated.Global
namespace Raven.Yabt.Database.Common.Configuration;

/// <summary>
///     Raven database session settings
/// </summary>
public class DatabaseSessionSettings
{
	/// <summary>
	///		The default time interval in seconds for waiting for the indexes to catch up with the saved changes
	///		after calling session.SaveChanges().
	///		If `null` then doesn't wait.
	///		The value must be ≥ <see cref="LogErrorIfSavingTakesMoreThan"/> 
	/// </summary>
	public int? WaitForIndexesAfterSaveChanges { get; private set; } = null;

	/// <summary>
	///     Log a trace warning if session.Save() takes more than specified in seconds,
	///		but less than in <see cref="LogErrorIfSavingTakesMoreThan"/>.
	///		It makes sense only if <see cref="WaitForIndexesAfterSaveChanges"/> is configured
	/// </summary>
	public int LogWarningIfSavingTakesMoreThan { get; private set; } = 15;

	/// <summary>
	///     Log an error if session.Save() takes more than specified in seconds.
	///		It makes sense only if <see cref="WaitForIndexesAfterSaveChanges"/> is configured.
	///		The value must be ≥ <see cref="LogWarningIfSavingTakesMoreThan"/>.
	/// </summary>
	public int LogErrorIfSavingTakesMoreThan { get; private set; } = 30;
}