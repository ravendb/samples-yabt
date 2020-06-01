using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.Common
{
	public abstract class BaseQueryService<TEntity> : BaseService<TEntity> where TEntity : IEntity
	{
		protected bool isSearchResult = false;

		protected BaseQueryService(IAsyncDocumentSession dbSession) : base(dbSession) { }

		[SuppressMessage("Compiler", "CS8602")] // Supress 'Dereference of a possibly null reference.', as compiler doesn't see a sophisticated validation of the type implementing 'ISearchable'
		protected virtual IRavenQueryable<TIndexModel> ApplySearch<TIndexModel>(IRavenQueryable<TIndexModel> query, string? search)
		{
			if (string.IsNullOrWhiteSpace(search) || !typeof(ISearchable).IsAssignableFrom(typeof(TIndexModel)))
				return query;

			return ApplySearch(query, s => (s as ISearchable).Search, search);
		}

		protected IRavenQueryable<T> ApplySearch<T>(IRavenQueryable<T> query, Expression<Func<T, object>> fieldExpression, string search)
		{
			search = search.Trim();

			// Generate a search string for just beginning of the words.
			// E.g. "David Smith-Lowe" becomes "David* Smith-Lowe*"
			string searchWildCards = Regex.Replace(search + " ", @"[\s,;:""{}[]|\\/`~!@#$%^&*()_=\+]+", "* ").Trim();

			isSearchResult = true;

			// boost exact matches more so they are displayed first
			return query
						.Search(fieldExpression, search.ToLower(), boost: 1000M)
						.Search(fieldExpression, searchWildCards.ToLower(), boost: 800M);
		}
	}
}
