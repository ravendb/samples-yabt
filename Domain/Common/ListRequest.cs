using System;
using System.ComponentModel.DataAnnotations;

namespace Raven.Yabt.Domain.Common
{
	public class ListRequest<TOrderEnum> where TOrderEnum : Enum
	{
#pragma warning disable CS8601 // Possible null reference assignment.
		public TOrderEnum OrderBy { get; set; } = default;
#pragma warning restore CS8601 // Possible null reference assignment.

		public OrderDirections OrderDirection { get; set; } = OrderDirections.Asc;

		[Range(0, int.MaxValue, ErrorMessage = "Must be greater than or equal to 0.")]
		public int PageIndex { get; set; } = 0;

		[Range(1, int.MaxValue, ErrorMessage = "Must be greater than or equal to 1.")]
		public int PageSize { get; set; } = int.MaxValue;
	}
}
