using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Raven.Yabt.Domain.Helpers;

public static class TypeExtensions
{
	/// <summary>
	///		Checks whether this (<paramref name="this"/>) is a closed type of a given generic type (<paramref name="openGeneric"/>)
	/// </summary>
	/// <remarks>
	///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/TypeExtensions.cs
	/// </remarks>
	public static bool IsClosedTypeOf(this Type @this, Type openGeneric)
	{
		return TypesAssignableFrom(@this).Any(t => t.IsGenericType && !@this.ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
	}

	/// <summary>
	///		Determines whether this type is assignable to <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type to test assignability to.</typeparam>
	/// <param name="this">The type to test.</param>
	/// <returns>True if this type is assignable to references of type
	/// <typeparamref name="T"/>; otherwise, False.</returns>
	/// <remarks>
	///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/TypeExtensions.cs
	/// </remarks>
	public static bool IsAssignableTo<T>(this Type @this)
	{
		if (@this == null)
		{
			throw new ArgumentNullException(nameof(@this));
		}

		return typeof(T).IsAssignableFrom(@this);
	}

	///  <summary>
	/// 	Returns an object that represents the specified method declared by the current type.
	///  </summary>
	///  <param name="this"> The type </param>
	///  <param name="methodName"> The name of the method </param>
	///  <param name="types"> [Optional] Parameters of the method </param>
	///  <returns>An object that represents the specified method, if found; otherwise, null.</returns>
	public static MethodInfo GetDeclaredMethod(this Type @this, string methodName, Type[]? types = null)
	{
		if (@this is null)
			throw new ArgumentNullException(nameof(@this));
		if (methodName is null)
			throw new ArgumentNullException(nameof(methodName));

		var foundMethod = types == null ? @this.GetMethod(methodName) : @this.GetMethod(methodName, types);

		if (foundMethod is null)
			throw new EntryPointNotFoundException();

		return foundMethod;
	}

	#region Auxiliary methods [PRIVATE, STATIC] ---------------------------

	/// <summary>
	///		Gets all types <paramref name="candidateType"/> is derived from
	/// </summary>
	/// <remarks>
	///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/TypeExtensions.cs
	/// </remarks>
	private static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
	{
		return candidateType.GetInterfaces().Concat(Across(candidateType, t => t.BaseType!));
	}

	/// <summary>
	///		Traverse across a set, taking the first item in the set, and a function to determine the next item.
	/// </summary>
	/// <typeparam name="T">The set type.</typeparam>
	/// <param name="first">The first item in the set.</param>
	/// <param name="next">A callback that will take the current item in the set, and output the next one.</param>
	/// <returns>An enumerable of the set.</returns>
	/// <remarks>
	///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/Traverse.cs
	/// </remarks>
	private static IEnumerable<T> Across<T>(T first, Func<T, T?> next) where T : class
	{
		var item = first;
		while (item != null)
		{
			yield return item;
			item = next(item);
		}
	}
	#endregion Auxiliary methods [PRIVATE, STATIC] ------------------------
}