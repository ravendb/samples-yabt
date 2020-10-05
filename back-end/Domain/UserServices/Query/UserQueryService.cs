using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DomainResults.Common;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models.Users;
using Raven.Yabt.Database.Models.Users.Indexes;
using Raven.Yabt.Domain.BacklogItemServices.Query.DTOs;
using Raven.Yabt.Domain.Common;
using Raven.Yabt.Domain.UserServices.Query.DTOs;

namespace Raven.Yabt.Domain.UserServices.Query
{
	public class UserQueryService : BaseQueryService<User>, IUserQueryService
	{
		public UserQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		public async Task<IDomainResult<UserGetByIdResponse>> GetById(string id)
		{
			var fullId = GetFullId(id);

			var user = await DbSession.LoadAsync<User>(fullId);
			if (user == null)
				return DomainResult.NotFound<UserGetByIdResponse>();

			return DomainResult.Success(user.ConvertToUserDto());
		}

		public async Task<List<UserListGetResponse>> GetList(UserListGetRequest dto)
		{
			var query = DbSession.Query<UserIndexedForList, Users_ForList>();

			query = ApplySearch(query, dto.Search);
			query = ApplySorting(query, dto);

			query = query.Skip(dto.PageIndex * dto.PageSize).Take(dto.PageSize);

			var ret = await (from u in query
							 select new UserListGetResponse
							 {
								 Id = u.Id,
								 FirstName = u.FirstName,
								 LastName = u.LastName,
								 ShortName = u.ShortName,
								 Email = u.Email,
								 RegistrationDate = u.RegistrationDate
							 }
						).ToListAsync();

			return ret;
		}

		private IRavenQueryable<UserIndexedForList> ApplySorting(IRavenQueryable<UserIndexedForList> query, UserListGetRequest dto)
		{
			if (dto.OrderBy == UsersOrderColumns.Default)
			{
				if (isSearchResult)
					return query;   // Use default order by releavnce
									// Otherwise descending sort by number
				dto.OrderBy = UsersOrderColumns.Name;
				dto.OrderDirection = OrderDirections.Asc;
			}

			return dto.OrderBy switch
			{
				UsersOrderColumns.Name				=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.FullName)			: query.OrderByDescending(t => t.FullName),
				UsersOrderColumns.Email				=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.Email)				: query.OrderByDescending(t => t.Email),
				UsersOrderColumns.RegistrationDate	=> dto.OrderDirection == OrderDirections.Asc ? query.OrderBy(t => t.RegistrationDate)	: query.OrderByDescending(t => t.RegistrationDate),
				_ => throw new NotImplementedException()
			};
		}
	}
}
