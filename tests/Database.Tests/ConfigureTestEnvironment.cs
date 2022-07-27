using System;

using Microsoft.Extensions.DependencyInjection;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using Raven.Yabt.Database.Infrastructure;

namespace Raven.Yabt.Database.Tests;

/// <summary>
///		Base test class to configure the environment (IoC and Raven)
/// </summary>
public abstract class ConfigureTestEnvironment : RavenTestDriver
{
	private readonly IServiceProvider _container;
	protected IAsyncTenantedDocumentSession DbSession => _container.GetRequiredService<IAsyncTenantedDocumentSession>();
	protected IDocumentStore DbStore => _container.GetRequiredService<IDocumentStore>();
	protected bool ThrowExceptionOnWrongTenant = true;
		
	private const string MyTenantId = "1-A";
	private const string NotMyTenantId = "2-A";
	protected bool IsMyTenantFlag = true;

	/// <summary>
	///		Get the ID of the current tenant		
	/// </summary>
	protected string GetCurrentTenantId() => IsMyTenantFlag ? MyTenantId : NotMyTenantId;

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

	/// <summary>
	///		Configure IoC, register all dependencies
	/// </summary>
	private void ConfigureIocContainer(IServiceCollection services)
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
			var session = new AsyncTenantedDocumentSession(docStore, GetCurrentTenantId, TimeSpan.FromSeconds(30), ThrowExceptionOnWrongTenant, new SessionOptions { NoCaching = true });
			return session as IAsyncTenantedDocumentSession;
		});
	}
}