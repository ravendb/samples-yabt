using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using Raven.Yabt.Database.Common.Configuration;
using Raven.Yabt.Domain.Infrastructure;

namespace Raven.Yabt.WebApi.Infrastructure;

public class DbSessionManagementFilter : IAsyncActionFilter
{
	private readonly IDbSessionSavingTimerWrapper _sessionSavingTimer;
	private readonly ILogger<DbSessionManagementFilter> _logger;
	private readonly int _logWarningIfSavingTakesMoreThan;
	private readonly int _logErrorIfSavingTakesMoreThan;

	public DbSessionManagementFilter(
		IDbSessionSavingTimerWrapper sessionSavingTimer,
		ILogger<DbSessionManagementFilter> logger,
		DatabaseSessionSettings? ravenSessionSettings)
	{
		_sessionSavingTimer = sessionSavingTimer;
		_logger = logger;
		_logWarningIfSavingTakesMoreThan= ravenSessionSettings?.LogWarningIfSavingTakesMoreThan	* 1000 ?? int.MaxValue;
		_logErrorIfSavingTakesMoreThan	= ravenSessionSettings?.LogErrorIfSavingTakesMoreThan	* 1000 ?? int.MaxValue;
	}

	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var resultContext = await next();

		if (resultContext.Exception == null || resultContext.ExceptionHandled)
		{
			var elapsedMilliseconds = await _sessionSavingTimer.SaveChangesWithTimerAsync();
				
			// Controlling saving time is important only if configured to wait indexes to update
			if (elapsedMilliseconds > Math.Min(_logWarningIfSavingTakesMoreThan, _logErrorIfSavingTakesMoreThan))
			{
				var str = $"SaveChanges() execution took {(elapsedMilliseconds / 1000):D}s";

				if		(elapsedMilliseconds > _logErrorIfSavingTakesMoreThan)		_logger.LogError(str);
				else if (elapsedMilliseconds > _logWarningIfSavingTakesMoreThan)	_logger.LogWarning(str);
			}
		}
	}
}