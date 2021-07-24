using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using Raven.Yabt.Database.Infrastructure;

namespace Raven.Yabt.Database.Migration.Tests
{
	/// <summary>
	///		Base test class to configure the environment (IoC and Raven)
	/// </summary>
	public abstract class ConfigureTestEnvironment : RavenTestDriver
	{
		private readonly IServiceProvider _container;
		protected IAsyncDocumentSession DbSession => _container.GetRequiredService<IAsyncDocumentSession>();

		/// <summary>
		///		The default c-tor initialising all the IoC interfaces
		/// </summary>
		protected ConfigureTestEnvironment()
		{
			var services = new ServiceCollection();
			// ReSharper disable once VirtualMemberCallInConstructor
			ConfigureIocContainer(services);
			_container = services.BuildServiceProvider();
		}

		protected override void PreInitialize(IDocumentStore store)
		{
			store.PreInitializeDocumentStore();
			store.Conventions.MaxNumberOfRequestsPerSession = 200;

			base.PreInitialize(store);
		}

		protected ILogger CreateLogger<T>() => _container.GetRequiredService<ILoggerFactory>().CreateLogger<T>();

		/// <summary>
		///		Configure IoC, register all dependencies
		/// </summary>
		private void ConfigureIocContainer(IServiceCollection services)
		{
			// Register the document store & session
			services.AddScoped(_ => GetDocumentStore());
			services.AddScoped(c => c.GetRequiredService<IDocumentStore>().OpenAsyncSession());
			
			// Register a Null Logger (see https://stackoverflow.com/a/47328428/968003)
			services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
		}
	}
}
