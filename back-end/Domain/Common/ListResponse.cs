using System.Collections.Generic;

namespace Raven.Yabt.Domain.Common;

public class ListResponse<T>
{
	/// <summary>
	///		The total number of records in the DB meeting the filtering conditions
	/// </summary>
	public int TotalRecords { get; set; }

	/// <summary>
	///		Zero-based page index
	/// </summary>
	public int PageIndex { get; set; }
	public int TotalPages { get; set; }

	public IList<T> Entries { get; set; }

	public ListResponse(IList<T> entries, int totalRecords, int currentPageIndex, int pageSize)
	{
		Entries = entries;
		TotalRecords = totalRecords;
		PageIndex = currentPageIndex;
		TotalPages = (totalRecords / pageSize) + (totalRecords % pageSize == 0 ? 0 : 1);
	}

	public ListResponse()
	{
		Entries = new List<T>();
	}
}