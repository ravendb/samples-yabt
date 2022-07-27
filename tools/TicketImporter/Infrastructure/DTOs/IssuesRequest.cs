#nullable disable  // Disable nullable check for a response DTO file
using System;

namespace Raven.Yabt.TicketImporter.Infrastructure.DTOs;

public class IssuesRequest
{
	/// <summary>
	///		Indicates which sorts of issues to return
	/// </summary>
	public Filters Filter { get; set; } = Filters.All;

	/// <summary>
	///		Indicates the state of the issues to return
	/// </summary>
	public States State { get; set; } = States.All;

	public SortOrder Sort { get; set; } = SortOrder.Created;
	public SortDirection Direction { get; set; } = SortDirection.Desc;

	/// <summary>
	///		Only issues updated at or after this time are returned. 
	/// </summary>
	public DateTime Since { get; set; } = DateTime.Today.AddDays(-2 * 365);

	public enum SortOrder
	{
		Created,
		Updated,
		Comments
	}

	public enum SortDirection
	{
		Asc,
		Desc
	}

	public enum States
	{
		Open,
		Closed,
		All
	}

	public enum Filters
	{
		/// <summary>
		///		Assigned to the current user
		/// </summary>
		Assigned,
		/// <summary>
		///		Created by the current user
		/// </summary>
		Created,
		/// <summary>
		///		Mentioned the current user
		/// </summary>
		Mentioned,
		/// <summary>
		///		Subscribed by the current user
		/// </summary>
		Subscribed,
		/// <summary>
		///		All issues the authenticated user can see
		/// </summary>
		All
	}
}