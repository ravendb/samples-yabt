using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Raven.Client.Documents.Conventions;
using Raven.Yabt.Database.Common;

namespace Raven.Yabt.Domain.Helpers
{
	internal static class SanitiseReferenceIdsExtension
	{
		internal static void RemoveEntityPrefixFromIds<T, TReference>(this IEnumerable<T> targets, params Expression<Func<T, TReference>>[] referenceMemberLambdas) where TReference : IEntity
		{
			foreach (var target in targets)
				target.RemoveEntityPrefixFromIds(referenceMemberLambdas);
		}

		internal static void RemoveEntityPrefixFromIds<T, TReference>(this T target, params Expression<Func<T, TReference>>[] referenceMemberLambdas) where TReference : IEntity
		{
			foreach (var referenceMember in referenceMemberLambdas)
				target.RemoveEntityPrefixFromIds(referenceMember);
		}

		internal static void RemoveEntityPrefixFromIds<T, TReference>(this T target, Expression<Func<T, TReference>> referenceMemberLambda) where TReference : IEntity
		{
			if (   !(referenceMemberLambda.Body is MemberExpression referenceMemberSelectorExpression)
				|| !(referenceMemberSelectorExpression.Member is PropertyInfo referenceProperty))
				return;
			
			// Read the current reference
			var referenceFunc = referenceMemberLambda.Compile();
			var reference = referenceFunc(target);

			if (reference == null)
				return;

			reference.RemoveEntityPrefixFromId();
		}

		internal static T RemoveEntityPrefixFromId<T>(this T target) where T: IEntity
		{
			// The new ID value without the entity prefix
			var newRefId = target?.Id?.GetShortId();

			if (newRefId == null)
				return target;
			var type = target!.GetType();
			
			var idProp = type.GetProperty(nameof(IEntity.Id));
			if (idProp == null)
				throw new NotImplementedException($"No public setter on '{nameof(IEntity.Id)}' property of '{type.Name}' type");

			idProp.SetValue(target, newRefId);

			return target;
		}

		/// <summary>
		///		Get a shorten document ID by dropping the prefix. E.g. for 'users/1-A' returns '1-A'
		/// </summary>
		/// <param name="fullId"> The full document ID (e.g. 'users/1-A') </param>
		/// <returns> A shorten ID (e.g. '1-A') </returns>
		internal static string? GetShortId(this string? fullId) => fullId?.Split('/').Last();

		/// <summary>
		///		Get full document ID for a given entity (e.g. for '1-A' returns 'users/1-A')
		/// </summary>
		/// <typeparam name="T"> The entity type (e.g. class `Users`) </typeparam>
		/// <param name="shortId"> The short ID (e.g. '1-A') </param>
		/// <returns> A full ID (e.g. 'users/1-A') </returns>
		internal static string GetFullId<T>(this string shortId) where T : IEntity
		{
			// In input we don't trust. Though I might be a bit paranoid, but this value can come from outside of the app and be passed to Raven
			if (!new Regex(@"^\d{1,19}\-[a-z]{1}$", RegexOptions.IgnoreCase).IsMatch(shortId))
				throw new ArgumentException("ID has incorrect format", nameof(shortId));

			// Pluralise the collection name by the Raven rules (e.g. for `User` class we get `users`)
			return $"{DocumentConventions.DefaultGetCollectionName(typeof(T))}/{shortId}";
		}

		/// <summary>
		///		Get a special form of the ID for using in the dynamic fields
		/// </summary>
		/// <remarks>
		///		Full ID can't be used due to https://issues.hibernatingrhinos.com/issue/RavenDB-15235.
		///		Short ID can't be used due to https://issues.hibernatingrhinos.com/issue/RavenDB-15234.
		/// </remarks>
		/// <typeparam name="T"> The entity type (e.g. class `Users`) </typeparam>
		/// <param name="shortId"> The short ID (e.g. '1-A') </param>
		/// <returns> A special ID for dynamic fields (e.g. 'users1-A') </returns>
		internal static string GetIdForDynamicField<T>(this string shortId) where T : IEntity
				=> shortId.GetFullId<T>().Replace("/", "").ToLower();
	}
}
