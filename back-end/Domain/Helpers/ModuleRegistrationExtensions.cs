using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Raven.Yabt.Domain.Helpers;

public static class ModuleRegistrationExtensions
{
	/// <summary>
	///     Register a service against all interfaces of <typeparamref name="T"/> with a life time scope <paramref name="lifetime"/>
	/// </summary>
	public static IServiceCollection RegisterAsImplementedInterfaces<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient, IEnumerable<Type>? skipInterfaces = null)
	{
		services.RegisterAsImplementedInterfaces(typeof(T), lifetime, skipInterfaces);
		return services;
	}

	/// <summary>
	///		Register a collection of types (<paramref name="types"/>) for their implemented interfaces
	/// </summary>
	public static IServiceCollection RegisterAsImplementedInterfaces(this IServiceCollection services, IEnumerable<Type> types, ServiceLifetime lifetime, ICollection<Type>? skipInterfaces = null)
	{
		foreach (Type type in types)
			services.RegisterAsImplementedInterfaces(type, lifetime, skipInterfaces);
		return services;
	}

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
}