using System;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using Raven.Yabt.Database.Infrastructure;

namespace Raven.Yabt.Database.Tests
{
	/// <summary>
	///		Base test class to configure the environment (IoC and Raven)
	/// </summary>
	public abstract class ConfigureTestEnvironment : RavenTestDriver
	{
		protected IServiceProvider Container { get; }
		protected IAsyncTenantedDocumentSession DbSession => Container.GetService<IAsyncTenantedDocumentSession>()!;
		protected bool ThrowExceptionOnWrongTenant = true;
		
		/// <summary>
		///		Get the ID of the current tenant		
		/// </summary>
		protected virtual string GetCurrentTenantId() => "1-A";

		/// <summary>
		///		The default c-tor initialising all the IoC interfaces
		/// </summary>
		protected ConfigureTestEnvironment()
		{
			var services = new ServiceCollection();
			// ReSharper disable once VirtualMemberCallInConstructor
			ConfigureIocContainer(services);
			Container = services.BuildServiceProvider();
		}

		protected override void PreInitialize(IDocumentStore store)
		{
			store.PreInitializeDocumentStore();
			store.Conventions.MaxNumberOfRequestsPerSession = 200;

			base.PreInitialize(store);
		}

		/// <summary>
		///		Configure IoC, register all dependencies
		/// </summary>
		protected void ConfigureIocContainer(IServiceCollection services)
		{
			// Register the document store & session
			services.AddScoped(_ =>
				{
					IDocumentStore store = GetDocumentStore();
					// Create all indexes
					IndexCreation.CreateIndexes(typeof(SetupDocumentStore).Assembly, store, null, store.Database);
					return store;
				});
			services.AddScoped(c =>
				{
					var docStore = c.GetRequiredService<IDocumentStore>();
					var session = new AsyncTenantedDocumentSession(docStore, GetCurrentTenantId, ThrowExceptionOnWrongTenant, new SessionOptions { NoCaching = true });
						session.Advanced.WaitForIndexesAfterSaveChanges();  // Wait on each change to avoid adding WaitForIndexing() in each test
					return session as IAsyncTenantedDocumentSession;
				});
		}
	}
}
