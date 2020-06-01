using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models.BacklogItem;
using Raven.Yabt.Database.Models.BacklogItem.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.ListQuery.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.CustomFieldServices.Query;
using Raven.Yabt.Domain.CustomFieldServices.Query.DTOs;
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

		public async Task<List<BacklogItemListGetResponse>> GetList(BacklogItemListGetRequest dto)
		{
			var query = DbSession.Query<BacklogItemIndexedForList, BacklogItems_ForList>();

			query = await ApplyFilters(query, dto);
			query = ApplySearch(query, dto.Search);
			query = ApplySorting(query, dto);

			query = query.Skip(dto.PageIndex * dto.PageSize).Take(dto.PageSize);

			return await query.As<BacklogItem>().Select(b => 
					new BacklogItemListGetResponse
					{ 
						Id = b.Id,
						Title = b.Title,
						Type = b.Type,
						Assignee = b.Assignee,
						// Have to re-calculate 'Created' and 'LastUpdated' server-side, as the entity model's fields get calculated client-side only 
						Created		= b.Modifications.OrderBy(m => m.Timestamp).FirstOrDefault() as ChangedByUserReference,
						LastUpdated = b.Modifications.OrderBy(m => m.Timestamp).LastOrDefault() as ChangedByUserReference
					}).ToListAsync();
		}

		private async Task<IRavenQueryable<BacklogItemIndexedForList>> ApplyFilters(IRavenQueryable<BacklogItemIndexedForList> query, BacklogItemListGetRequest dto)
		{
			if (dto.Type != BacklogItemType.Unknown)
				query = query.Where(t => t.Type == dto.Type);

			if (dto.ModifiedByTheCurrentUserOnly)
			{
				var userKey = GetModificationKeyForUser();
				query = query.Where(t => t.ModifiedByUser[userKey] > DateTime.MinValue);
			}

			if (!string.IsNullOrEmpty(dto.AssignedUserId))
				query = query.Where(t => t.AssignedUserId == dto.AssignedUserId);

			// Special filters for user's modifications (modified by, created by, etc.)
			if (dto.UserModification != null)
			{
				var userKey = GetModificationKeyForUser(dto.UserModification.UserId);

				query = dto.UserModification.Type switch
				{
					BacklogItemModification.ModificationType.Any		 => query.Where(t => t.ModifiedByUser[userKey] > DateTime.MinValue),
					BacklogItemModification.ModificationType.CreatedOnly => query.Where(t => t.CreatedByUserId == dto.UserModification.UserId),
					_ => throw new NotSupportedException(),
				};
			}

			// Special filters by custom fields
			if (dto.CustomField != null)
			{
				var customFields = await _customFieldService.GetArray(new CustomFieldListGetRequest { Ids = dto.CustomField.Keys });
				foreach (var customField in customFields)
				{
					var val = dto.CustomField.First(cf => cf.Key == customField.Id).Value;
					var customFieldIdForIndex = "F" + customField.Id;  // Note: Don't concatenate the suffix in the Where/Search expression, Raven.Linq would fail with a mysterious error
					
					query = customField.FieldType switch
					{
						// Search in text custom fields
						CustomFieldType.Text => ApplySearch(query, b => b.CustomFields[customFieldIdForIndex], val),
						// Exact match in others
						_ => query.Where(t => t.CustomFields[customFieldIdForIndex] == val),
					};
				}
			}

			return query;
		}

		private IRavenQueryable<BacklogItemIndexedForList> ApplySorting(IRavenQueryable<BacklogItemIndexedForList> query, BacklogItemListGetRequest dto)
		{
			if (dto.OrderBy == BacklogItemsOrderColumns.Default)
			{
				if (isSearchResult)
					return query;   // Use default order by releavnce
				// Otherwise descending sort by number
				dto.OrderBy = BacklogItemsOrderColumns.Number;
				dto.OrderDirection = OrderDirections.Desc;
			}

			var userKey = GetModificationKeyForUser();

			return dto.OrderBy switch
			{
				BacklogItemsOrderColumns.Number =>				dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Id) : query.OrderByDescending(t => t.Id),
				BacklogItemsOrderColumns.Title =>				dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
				BacklogItemsOrderColumns.TimestampCreated =>	dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.CreatedTimestamp) : query.OrderByDescending(t => t.CreatedTimestamp),
				BacklogItemsOrderColumns.TimestampLastModified=>dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.LastUpdatedTimestamp) : query.OrderByDescending(t => t.LastUpdatedTimestamp),
				BacklogItemsOrderColumns.TimestampModifiedByCurrentUser =>
																dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.ModifiedByUser[userKey]) : query.OrderByDescending(t => t.ModifiedByUser[userKey]),
				_ => throw new NotImplementedException()
			};
		}

		private string GetModificationKeyForUser(string? userId = null) => $"M{userId ?? _userResolver.GetCurrentUserId()}";
	}
}