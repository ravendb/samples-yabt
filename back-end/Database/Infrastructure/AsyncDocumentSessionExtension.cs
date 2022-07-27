using System;
using System.Linq;
using System.Text.RegularExpressions;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Database.Infrastructure;

public static class AsyncDocumentSessionExtension
{
	/// <summary>
	///		Gets a shorten document ID by dropping the prefix. E.g. for 'users/1-A' returns '1-A'
	/// </summary>
	/// <param name="fullId"> The full document ID (e.g. 'users/1-A') </param>
	/// <returns> A shorten ID (e.g. '1-A') </returns>
	public static string? GetShortId(this string? fullId) => fullId?.Split('/').Last();

	/// <summary>
	///		Gets full document ID for a given entity (e.g. for '1-A' returns 'users/1-A')
	/// </summary>
	/// <typeparam name="T"> The entity type (e.g. class `Users`) </typeparam>
	/// <param name="advSession"> Advanced session props to resolve conventions for converting the ID </param>
	/// <param name="shortId"> The short ID (e.g. '1-A') </param>
	/// <returns> A full ID (e.g. 'users/1-A') </returns>
	public static string GetFullId<T>(this IAsyncAdvancedSessionOperations advSession, string shortId) where T : IEntity
	{
		// In input we don't trust. Though I might be a bit paranoiac, but this value can come from outside of the app and be passed to Raven
		if (!new Regex(@"^\d{1,19}\-[a-z]{1}$", RegexOptions.IgnoreCase).IsMatch(shortId))
			throw new ArgumentException("ID has incorrect format", nameof(shortId));

		// Pluralise the collection name (e.g. 'User' becomes 'Users', 'Person' becomes 'People')
		var pluralisedName = advSession.DocumentStore.Conventions.GetCollectionName(typeof(T));
		// Fix the later case - converts 'Users' to 'users', 'BacklogItems' to 'backlogItems'
		var prefix = advSession.DocumentStore.Conventions.TransformTypeCollectionNameToDocumentIdPrefix(pluralisedName);

		return $"{prefix}/{shortId}";
	}
}