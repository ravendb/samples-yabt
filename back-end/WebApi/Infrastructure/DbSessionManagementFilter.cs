using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using Raven.Client.Documents.Session;
using Raven.Yabt.Database.Configuration;
using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.WebApi.Infrastructure
{
	public class DbSessionManagementFilter : IAsyncActionFilter
	{
		private readonly IAsyncDocumentSession _dbSession;
		private readonly IPatchOperationsExecuteAsync _patchOperations;
		private readonly ILogger<DbSessionManagementFilter> _logger;
		private readonly int _logWarningIfSavingTakesMoreThan;
		private readonly int _logErrorIfSavingTakesMoreThan;

		public DbSessionManagementFilter(
			IAsyncDocumentSession dbSession,
			IPatchOperationsExecuteAsync patchOperations,
			ILogger<DbSessionManagementFilter> logger,
			DatabaseSettings ravenDbSettings)
		{
			_dbSession = dbSession ?? throw new ArgumentNullException(nameof(dbSession));
			_patchOperations = patchOperations;
			_logger = logger;
			_logWarningIfSavingTakesMoreThan= ravenDbSettings.LogWarningIfSavingTakesMoreThan * 1000;
			_logErrorIfSavingTakesMoreThan	= ravenDbSettings.LogErrorIfSavingTakesMoreThan	  * 1000;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			ActionExecutedContext resultContext = await next();

			if (resultContext.Exception == null || resultContext.ExceptionHandled)
			{
				await OnAfterActionExecutionAsync();
			}
		}

		/// <summary>
		///		Saving any changes to the DB on finishing the method execution
		/// </summary>
		private async Task OnAfterActionExecutionAsync()
		{
			if (HasSessionChanges())
			{
				Stopwatch sw = Stopwatch.StartNew();
				// Saving. Note it throws an exception on any error
				await _dbSession.SaveChangesAsync();
				// Run deferred patches not waiting for them to finish
				await _patchOperations.SendAsyncDeferredPatchByQueryOperations();
				sw.Stop();

				if (sw.ElapsedMilliseconds > _logWarningIfSavingTakesMoreThan)
				{
					string str = $"SaveChanges() execution took {(sw.ElapsedMilliseconds / 1000):D}s";

					if		(sw.ElapsedMilliseconds > _logErrorIfSavingTakesMoreThan)	_logger.LogError(str);
					else if (sw.ElapsedMilliseconds > _logWarningIfSavingTakesMoreThan)	_logger.LogWarning(str);
				}
			}
		}

		private bool HasSessionChanges()
		{
			if (_dbSession?.Advanced == null)
				return false;
			
			// Check if a record was created/updated
			if (/* Use it instead of '!_dbSession.HasChanges' while https://issues.hibernatingrhinos.com/issue/RavenDB-15690 is not fixed */
				_dbSession.Advanced.WhatChanged()?.Values.Any() == true 
				// Check if there is a PATCH request
				|| (_dbSession.Advanced is InMemoryDocumentSessionOperations operations && operations.DeferredCommandsCount > 0)
			)
				return true;

			return _patchOperations.AreDeferredPatchesForExecution;
		}
	}
}
