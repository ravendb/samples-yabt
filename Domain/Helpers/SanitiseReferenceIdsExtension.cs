using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Raven.Client.Documents.Conventions;
using Raven.Yabt.Database.Common;
using Raven.Yabt.Database.Common.References;

namespace Raven.Yabt.Domain.Helpers
{
	internal static class SanitiseReferenceIdsExtension
	{
		internal static void RemoveEntityPrefixFromIds<T, TReference>(this IEnumerable<T> targets, params Expression<Func<T, TReference>>[] referenceMemberLamdas) where TReference : IEntityReference
		{
			foreach (var target in targets)
				target.RemoveEntityPrefixFromIds(referenceMemberLamdas);
		}

		internal static void RemoveEntityPrefixFromIds<T, TReference>(this T target, params Expression<Func<T, TReference>>[] referenceMemberLamdas) where TReference : IEntityReference
		{
			foreach (var referenceMember in referenceMemberLamdas)
				target.RemoveEntityPrefixFromIds(referenceMember);
		}

		internal static void RemoveEntityPrefixFromIds<T, TReference>(this T target, Expression<Func<T, TReference>> referenceMemberLamda) where TReference : IEntityReference
		{
			if (!(referenceMemberLamda.Body is MemberExpression referenceMemberSelectorExpression))
				return;

			if (!(referenceMemberSelectorExpression.Member is PropertyInfo referenceProperty))
				return;
			
			// Read the current reference
			var referenceFunc = referenceMemberLamda.Compile();
			var reference = referenceFunc(target);

			if (reference == null)
				return;

			reference.RemoveEntityPrefixFromId();
		}

		internal static T RemoveEntityPrefixFromId<T>(this T target) where T: IEntityReference
		{
			// The new ID value without the entity prefix
			var newRefId = target?.Id?.ShortenId();

			if (newRefId == null)
				return target;
			var type = target!.GetType();
			
			var idProp = type.GetProperty(nameof(IEntityReference.Id));
			if (idProp == null)
				throw new NotImplementedException($"No public setter on '{nameof(IEntityReference.Id)}' property of '{type.Name}' type");

			idProp.SetValue(target, newRefId, null);

			return target;
		}

		internal static string? ShortenId(this string? fullId) => fullId?.Split('/').Last();

		internal static string GetFullId<T>(this string id) where T: IEntity
				=> $"{DocumentConventions.DefaultGetCollectionName(typeof(T))}/{id}";

		internal static string GetIdForDynamicField<T>(this string id) where T : IEntity
				=> id.GetFullId<T>().Replace("/", "").ToLower();
	}
}
