using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Raven.Yabt.Domain.Infrastructure
{
	public static class ModuleRegistrationExtensions
	{
		/// <summary>
		///     Register a service against all interfaces of <typeparamref name="T"/> with a life time scope <paramref name="lifetime"/>
		/// </summary>
		public static void RegisterAsImplementedInterfaces<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient, IEnumerable<Type>? skipInterfaces = null)
		{
			services.RegisterAsImplementedInterfaces(typeof(T), lifetime, skipInterfaces);
		}

		/// <summary>
		///		Register a collection of types (<paramref name="types"/>) for their implemented interfaces
		/// </summary>
		public static void RegisterAsImplementedInterfaces(this IServiceCollection services, IEnumerable<Type> types, ServiceLifetime lifetime, IEnumerable<Type>? skipInterfaces = null)
		{
			foreach (Type type in types)
				services.RegisterAsImplementedInterfaces(type, lifetime, skipInterfaces);
		}

		/// <summary>
		///		Register all modules (classes derived from <see cref="ModuleRegistration"/>) in the specified assembly
		/// </summary>
		public static void RegisterModules(this IServiceCollection services, Assembly assembly)
		{
			foreach (Type tp in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ModuleRegistration))))
			{
				if (Activator.CreateInstance(tp) is ModuleRegistration module)
					module.Configure(services);
			}
		}

		/// <summary>
		///		Determines whether the candidate type (<paramref name="type"/>) supports any base or interface that closes the provided generic type (<paramref name="openGeneric"/>)
		/// </summary>
		/// <remarks>
		///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/TypeExtensions.cs
		/// </remarks>
		public static bool IsClosedTypeOf(this Type type, Type openGeneric)
		{
			IEnumerable<Type> candidates = TypesAssignableFrom(type);
			return candidates.Any(t => t.GetTypeInfo().IsGenericType && !type.GetTypeInfo().ContainsGenericParameters && t.GetGenericTypeDefinition() == openGeneric);
		}

		#region Auxiliary methods [PRIVATE, STATIC] ---------------------------

		/// <summary>
		///		This registers the type against any public interfaces (other than IDisposable) implemented by the class
		/// </summary>
		/// <remarks>
		///		The implementation was copied from https://github.com/JonPSmith/NetCore.AutoRegisterDi/blob/master/NetCore.AutoRegisterDi/AutoRegisterHelpers.cs
		/// </remarks>
		private static void RegisterAsImplementedInterfaces(this IServiceCollection services, Type classType, ServiceLifetime lifetime, IEnumerable<Type>? skipInterfaces = null)
		{
			IEnumerable<Type> interfaces = classType.GetTypeInfo().ImplementedInterfaces
													.Where(i => i != typeof(IDisposable) && i.IsPublic);
			if (skipInterfaces != null)
				interfaces = interfaces.Where(i => skipInterfaces.All(ai => ai != i));

			foreach (Type interfaceType in interfaces)
				services.Add(new ServiceDescriptor(interfaceType, classType, lifetime));
		}

		/// <summary>
		///		Gets all types <paramref name="candidateType"/> is derived from
		/// </summary>
		/// <remarks>
		///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/TypeExtensions.cs
		/// </remarks>
		[SuppressMessage("Compiler", "CS8603")] // Pretty sure that 't.GetTypeInfo().BaseType' won't be null :)
		private static IEnumerable<Type> TypesAssignableFrom(Type candidateType)
		{
			return candidateType.GetTypeInfo().ImplementedInterfaces
								.Concat(
										Across(candidateType, t => t.GetTypeInfo().BaseType)
								);
		}

		/// <summary>
		///		Returns items meeting condition in <paramref name="next"/>
		/// </summary>
		/// <remarks>
		///		The implementation was copied from Autofac - https://github.com/autofac/Autofac/blob/develop/src/Autofac/Util/Traverse.cs
		/// </remarks>
		private static IEnumerable<T> Across<T>(T first, Func<T, T> next) where T : class
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
}