namespace Raven.Yabt.Database.Models;

/// <summary>
///		Interface with a searchable field
/// </summary>
public interface ISearchable
{
	/// <summary>
	///		String for searching in relevant text fields
	/// </summary>
	string Search { get; set; }
}