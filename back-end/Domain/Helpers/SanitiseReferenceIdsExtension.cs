using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Raven.Client.Documents.Conventions;
using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Models;

namespace Raven.Yabt.Domain.Helpers
{
	internal static class SanitiseReferenceIdsExtension
	{
		/// <summary>
		///		Removes the prefix from <see cref="IEntityReference.Id"/> properties of <paramref name="referenceMemberLambdas"/>
		/// </summary>
		internal static void RemoveEntityPrefixFromIds<T, TReference>(this IEnumerable<T> targets, params Expression<Func<T, TReference?>>[] referenceMemberLambdas) 
			where TReference : class, IEntityReference
		{
			foreach (var target in targets)
				target.RemoveEntityPrefixFromIds(referenceMemberLambdas);
		}

		/// <summary>
		///		Removes the prefix from <see cref="IEntityReference.Id"/> properties of <paramref name="referenceMemberLambdas"/>
		/// </summary>
		/// <remarks>
		///		Note: it mutates the <see cref="IEntityReference.Id"/> properties of <paramref name="target"/> 
		/// </remarks>
		internal static T RemoveEntityPrefixFromIds<T, TReference>(this T target, params Expression<Func<T, TReference?>>[] referenceMemberLambdas) 
			where TReference : class, IEntityReference
		{
			foreach (var referenceMember in referenceMemberLambdas)
				target.RemoveEntityPrefixFromIds(referenceMember);
			return target;
		}

		private static void RemoveEntityPrefixFromIds<T, TReference>(this T target, Expression<Func<T, TReference?>> referenceMemberLambda) 
			where TReference : class, IEntityReference
		{
			if (   !(referenceMemberLambda.Body is MemberExpression referenceMemberSelectorExpression)
				|| !(referenceMemberSelectorExpression.Member is PropertyInfo))
				return;
			
			// Read the current reference
			var referenceFunc = referenceMemberLambda.Compile();
			var reference = referenceFunc(target);
			// Remove the suffix
			reference?.RemoveEntityPrefixFromId();
		}

		/// <summary>
		///		Removes the prefix from <see cref="IEntityReference.Id"/> property (e.g. for "users/1-A" returns "1-A")
		/// </summary>
		/// <remarks>
		///		Note: it mutates the <see cref="IEntityReference.Id"/> property of <paramref name="target"/> 
		/// </remarks>
		internal static T RemoveEntityPrefixFromId<T>(this T target) where T: IEntityReference
		{
			// The new ID value without the entity prefix
			var newRefId = target.Id?.GetShortId();

			if (newRefId == null)
				return target;
			var type = target!.GetType();
			
			var idProp = type.GetProperty(nameof(IEntityReference.Id));
			if (idProp == null)
				throw new NotImplementedException($"No '{nameof(IEntityReference.Id)}' property of '{type.Name}' type");

			idProp.SetValue(target, newRefId);

			return target;
		}

		/// <summary>
		///		Gets a shorten document ID by dropping the prefix. E.g. for 'users/1-A' returns '1-A'
		/// </summary>
		/// <param name="fullId"> The full document ID (e.g. 'users/1-A') </param>
		/// <returns> A shorten ID (e.g. '1-A') </returns>
		internal static string? GetShortId(this string? fullId) => fullId?.Split('/').Last();

		/// <summary>
		///		Gets full document ID for a given entity (e.g. for '1-A' returns 'users/1-A')
		/// </summary>
		/// <typeparam name="T"> The entity type (e.g. class `Users`) </typeparam>
		/// <param name="session"> Session to resolve conventions for converting the ID </param>
		/// <param name="shortId"> The short ID (e.g. '1-A') </param>
		/// <returns> A full ID (e.g. 'users/1-A') </returns>
		internal static string GetFullId<T>(this IAsyncDocumentSession session, string shortId) where T : IEntity
		{
			// In input we don't trust. Though I might be a bit paranoiac, but this value can come from outside of the app and be passed to Raven
			if (!new Regex(@"^\d{1,19}\-[a-z]{1}$", RegexOptions.IgnoreCase).IsMatch(shortId))
				throw new ArgumentException("ID has incorrect format", nameof(shortId));

			// Pluralise the collection name (e.g. 'User' becomes 'Users', 'Person' becomes 'People')
			var pluralisedName = DocumentConventions.DefaultGetCollectionName(typeof(T));
			// Fix the later case - converts 'Users' to 'users', 'BacklogItems' to 'backlogItems'
			var prefix = session.Advanced.DocumentStore.Conventions.TransformTypeCollectionNameToDocumentIdPrefix(pluralisedName);

			return $"{prefix}/{shortId}";
		}
	}
}
