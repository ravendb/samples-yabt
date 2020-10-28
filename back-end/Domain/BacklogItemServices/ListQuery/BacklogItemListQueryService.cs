using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItems;
using Raven.Yabt.Database.Models.BacklogItems.Indexes;
using Raven.Yabt.Database.Models.CustomFields;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;
using Raven.Yabt.Domain.Helpers;
using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.Domain.BacklogItemServices.ListQuery
{
	public class BacklogItemListQueryService : BaseQueryService<BacklogItem>, IBacklogItemListQueryService
	{
		private readonly ICustomFieldQueryService _customFieldService;
		private readonly ICurrentUserResolver _userResolver;

		public BacklogItemListQueryService (IAsyncDocumentSession dbSession,
											ICustomFieldQueryService customFieldService, 
											ICurrentUserResolver userResolver) : base(dbSession)
		{
			_customFieldService = customFieldService;
			_userResolver = userResolver;
		}
		
		public async Task<ListResponse<BacklogItemListGetResponse>> GetList(BacklogItemListGetRequest dto)
		{
			var query = DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>();

			query = await ApplyFilters(query, dto);
			query = ApplySearch(query, dto.Search);

			var totalRecords = await query.CountAsync();

			query = ApplySorting(query, dto);

			query = query.Skip(dto.PageIndex * dto.PageSize).Take(dto.PageSize);

			var ret = await (from b in query.As<BacklogItem>()
							select new BacklogItemListGetResponse
							{ 
								Id = b.Id,
								Title = b.Title,
								Type = b.Type,
								Assignee = b.Assignee,
								// Have to re-calculate 'Created' and 'LastUpdated' server-side, as the entity model's fields get calculated client-side only 
								Created		= b.ModifiedBy.OrderBy(m => m.Timestamp).FirstOrDefault() as ChangedByUserReference,
								LastUpdated = b.ModifiedBy.OrderBy(m => m.Timestamp).LastOrDefault() as ChangedByUserReference
							}
						).ToListAsync();
			ret.RemoveEntityPrefixFromIds(r => r.Assignee, r => r.Created.ActionedBy, r => r.LastUpdated.ActionedBy);

			return new ListResponse<BacklogItemListGetResponse>(ret, totalRecords, dto.PageIndex, dto.PageSize);
		}

		private async Task<IRavenQueryable<BacklogItemIndexedForList>> ApplyFilters(IRavenQueryable<BacklogItemIndexedForList> query, BacklogItemListGetRequest dto)
		{
			if (dto.Type != BacklogItemType.Unknown)
				query = query.Where(t => t.Type == dto.Type);

			if (dto.Tags?.Any() == true)
				foreach (var tag in dto.Tags)
					query = query.Where(e => e.Tags!.Contains(tag));					// Note: [Tags] is a nullable field, but when the LINQ gets converted to RQL the potential NULLs get handled 

			if (dto.MentionsOfTheCurrentUserOnly)
			{
				var userId = GetUserIdForDynamicField();
				query = query.Where(t => t.MentionedUser![userId] > DateTime.MinValue);	// Note: [MentionedUser] is a nullable field, but when the LINQ gets converted to RQL the potential NULLs get handled
			}
			else if (dto.ModifiedByTheCurrentUserOnly)
			{
				var userIdForDynamicField = GetUserIdForDynamicField();
				query = query.Where(t => t.ModifiedByUser[userIdForDynamicField] > DateTime.MinValue);
			}

			if (!string.IsNullOrEmpty(dto.AssignedUserId))
			{
				var fullUserId = DbSession.GetFullId<User>(dto.AssignedUserId);
				query = query.Where(t => t.AssignedUserId == fullUserId);
			}

			// Special filters for user's modifications (modified by, created by, etc.)
			if (dto.UserModification != null)
			{
				var userIdForDynamicField = GetUserIdForDynamicField(dto.UserModification.UserId);
				var fullUserId = DbSession.GetFullId<User>(dto.UserModification.UserId);

				query = dto.UserModification.Type switch
				{
					BacklogItemModification.ModificationType.Any		 => query.Where(t => t.ModifiedByUser[userIdForDynamicField] > DateTime.MinValue),
					BacklogItemModification.ModificationType.CreatedOnly => query.Where(t => t.CreatedByUserId == fullUserId),
					_ => throw new NotSupportedException(),
				};
			}

			// Special filters by custom fields
			if (dto.CustomField != null)
			{
				var customFields = await _customFieldService.GetArray(new CustomFieldListGetRequest { Ids = dto.CustomField.Keys });
				foreach (var customField in customFields)
				{
					if (string.IsNullOrEmpty(customField.Id))
						continue;
					var val = dto.CustomField.First(cf => cf.Key == customField.Id).Value;
					var customFieldIdForIndex = DbSession.GetIdForDynamicField<CustomField>(customField.Id);
					var op = GetCustomFieldExpression(ref val);

					query = customField.FieldType switch
					{
						// Search in text custom fields
						CustomFieldType.Text	=> ApplySearch(query, b => b.CustomFields![customFieldIdForIndex], val),
						CustomFieldType.Numeric =>
						op switch
						{
							FieldOperators.GreaterThan		  => query.Where(b => (decimal)b.CustomFields![customFieldIdForIndex] >  decimal.Parse(val)),
							FieldOperators.GreaterThanOrEqual => query.Where(b => (decimal)b.CustomFields![customFieldIdForIndex] >= decimal.Parse(val)),
							FieldOperators.LessThan			  => query.Where(b => (decimal)b.CustomFields![customFieldIdForIndex] <  decimal.Parse(val)),
							FieldOperators.LessThanOrEqual	  => query.Where(b => (decimal)b.CustomFields![customFieldIdForIndex] <= decimal.Parse(val)),
							_								  => query.Where(b => (decimal)b.CustomFields![customFieldIdForIndex] == decimal.Parse(val)),
						},
						CustomFieldType.Date =>
						op switch
						{
							FieldOperators.GreaterThan		  => query.Where(b => (DateTime)b.CustomFields![customFieldIdForIndex] >  DateTime.Parse(val)),
							FieldOperators.GreaterThanOrEqual => query.Where(b => (DateTime)b.CustomFields![customFieldIdForIndex] >= DateTime.Parse(val)),
							FieldOperators.LessThan			  => query.Where(b => (DateTime)b.CustomFields![customFieldIdForIndex] <  DateTime.Parse(val)),
							FieldOperators.LessThanOrEqual	  => query.Where(b => (DateTime)b.CustomFields![customFieldIdForIndex] <= DateTime.Parse(val)),
							_								  => query.Where(b => (DateTime)b.CustomFields![customFieldIdForIndex] == DateTime.Parse(val)),
						},
						// Exact match in others
						_ => query.Where(t => t.CustomFields![customFieldIdForIndex].ToString() == val),
					};
				}
			}

			return query;
		}

		private static FieldOperators GetCustomFieldExpression(ref string val)
		{
			var supportedOperators = new[] { "lt", "lte", "gt", "gte", "eq" };
			var regExMatch = new Regex(@$"^({string.Join("|", supportedOperators)})\|", RegexOptions.IgnoreCase).Match(val.ToString());
			var op = string.Empty;
			if (regExMatch.Success)
			{
				op = regExMatch.Groups[1].Value;    // See why group 1 - https://stackoverflow.com/a/6376236/968003
				val = val.Replace(op + "|", "");
			}

			return op switch
			{
				"lt"  => FieldOperators.LessThan,
				"lte" => FieldOperators.LessThanOrEqual,
				"gt"  => FieldOperators.GreaterThan,
				"gte" => FieldOperators.GreaterThanOrEqual,
				_	  => FieldOperators.Equal
			};
		}

		private IRavenQueryable<BacklogItemIndexedForList> ApplySorting(IRavenQueryable<BacklogItemIndexedForList> query, BacklogItemListGetRequest dto)
		{
			if (dto.OrderBy == BacklogItemsOrderColumns.Default)
			{
				if (isSearchResult)
					return query;   // Use default order by relevance
				// Otherwise descending sort by number
				dto.OrderBy = BacklogItemsOrderColumns.Number;
				dto.OrderDirection = OrderDirections.Desc;
			}

			var userKey = GetUserIdForDynamicField();

			return dto.OrderBy switch
			{
				BacklogItemsOrderColumns.Number =>				dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Id) : query.OrderByDescending(t => t.Id),
				BacklogItemsOrderColumns.Title =>				dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
				BacklogItemsOrderColumns.TimestampCreated =>	dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.CreatedTimestamp) : query.OrderByDescending(t => t.CreatedTimestamp),
				BacklogItemsOrderColumns.TimestampLastModified=>dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.LastUpdatedTimestamp) : query.OrderByDescending(t => t.LastUpdatedTimestamp),
				BacklogItemsOrderColumns.TimestampModifiedByCurrentUser =>
																dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.ModifiedByUser[userKey]) : query.OrderByDescending(t => t.ModifiedByUser[userKey]),
				BacklogItemsOrderColumns.TimestampMentionsOfCurrentUser =>
																dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.MentionedUser![userKey]) : query.OrderByDescending(t => t.MentionedUser![userKey]),
				_ => throw new NotImplementedException()
			};
		}

		private string GetUserIdForDynamicField(string? userId = null) => DbSession.GetIdForDynamicField<User>(userId ?? _userResolver.GetCurrentUserId());

		private enum FieldOperators
		{
			Equal,
			LessThan,
			LessThanOrEqual,
			GreaterThan,
			GreaterThanOrEqual
		}
	}
}