using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Raven.Yabt.TicketImporter.Helpers;

/// <summary>
///     Build a query string with support of arrays
/// </summary>
/// <remarks>
///     As QueryHelpers.AddQueryString doesn't support array type parameter (https://github.com/dotnet/aspnetcore/issues/7945),
///     got the official MS solution from .NET Core v5 (https://github.com/dotnet/aspnetcore/blob/aeb718ece64b0935b44f6c42e6df0a874d841439/src/Http/Http.Extensions/src/QueryBuilder.cs)
/// </remarks>
internal class QueryBuilder : IEnumerable<KeyValuePair<string, string>>
{
	private readonly IList<KeyValuePair<string, string>> _params;

	public QueryBuilder()
	{
		_params = new List<KeyValuePair<string, string>>();
	}

	public QueryBuilder(IEnumerable<KeyValuePair<string, string>> parameters)
	{
		_params = new List<KeyValuePair<string, string>>(parameters);
	}

	public QueryBuilder(IEnumerable<KeyValuePair<string, StringValues>> parameters)
		: this(parameters.SelectMany(kvp => kvp.Value, (kvp, v) => KeyValuePair.Create(kvp.Key, v)))
	{

	}

	public void Add(string key, IEnumerable<string> values)
	{
		foreach (var value in values)
		{
			_params.Add(new KeyValuePair<string, string>(key, value));
		}
	}

	public void Add(string key, string value)
	{
		_params.Add(new KeyValuePair<string, string>(key, value));
	}

	public override string ToString()
	{
		var builder = new StringBuilder();
		var first = true;
		for (var i = 0; i < _params.Count; i++)
		{
			var pair = _params[i];
			builder.Append(first ? "?" : "&");
			first = false;
			builder.Append(UrlEncoder.Default.Encode(pair.Key));
			builder.Append("=");
			builder.Append(UrlEncoder.Default.Encode(pair.Value));
		}

		return builder.ToString();
	}

	public QueryString ToQueryString()
	{
		return new QueryString(ToString());
	}

	public override int GetHashCode()
	{
		return ToQueryString().GetHashCode();
	}

	public override bool Equals(object? obj)
	{
		return ToQueryString().Equals(obj);
	}

	public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
	{
		return _params.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _params.GetEnumerator();
	}
}