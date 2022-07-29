using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using Raven.Yabt.Database.Common.References;
using Raven.Yabt.Database.Infrastructure;

namespace Raven.Yabt.Domain.Helpers;

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
		var type = target.GetType();
			
		var idProp = type.GetProperty(nameof(IEntityReference.Id));
		if (idProp == null)
			throw new NotImplementedException($"No '{nameof(IEntityReference.Id)}' property of '{type.Name}' type");

		idProp.SetValue(target, newRefId);

		return target;
	}
	
	/// <summary>
	///		Remove suspicious symbols and convert to UPPER case 
	///		Replace invalid characters with empty strings. Can't pass it as a parameter, as string parameters get wrapped in '\"' when inserted
	/// </summary>
	internal static string GetSanitisedIdForPatchQuery(this string id) 
		=> Regex.Replace(id, @"[^\w\.@-]", "").ToUpper();
}