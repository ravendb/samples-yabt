using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Yabt.Database.Infrastructure;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Domain.Common;

public abstract class BaseQueryService<TEntity> : BaseService<TEntity> where TEntity : IEntity
{
	protected bool IsSearchResult;

	protected BaseQueryService(IAsyncTenantedDocumentSession dbSession) : base(dbSession) { }

	protected virtual IRavenQueryable<TIndexModel> ApplySearch<TIndexModel>(IRavenQueryable<TIndexModel> query, string? search) where TIndexModel: ISearchable
	{
		return ApplySearch(query, s => s.Search, search);
	}

	protected IRavenQueryable<T> ApplySearch<T>(IRavenQueryable<T> query, Expression<Func<T, object?>> fieldExpression, string? search)
	{
		if (string.IsNullOrWhiteSpace(search))
			return query;

		search = search.Trim();

		// Generate a search string for just beginning of the words.
		// E.g. "David Smith-Lowe" becomes "David* Smith-Lowe*"
		string searchWildCards = Regex.Replace(search + " ", @"[\s,;:""{}[]|\\/`~!@#$%^&*()_=\+]+", "* ").Trim();

		IsSearchResult = true;

		// boost exact matches more so they are displayed first
		return query
		       .Search(fieldExpression, search.ToLower(), boost: 1000M)
		       .Search(fieldExpression, searchWildCards.ToLower(), boost: 800M);
	}
}