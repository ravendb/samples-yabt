using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Raven.Yabt.WebApi.Infrastructure.StartupTasks;

/// <summary>
///		Handle failed or incomplete start-up tasks.
///		Return an error response code if the startup tasks have not finished
/// </summary>
/// <remarks>
///		Based on https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-4-using-health-checks/
/// </remarks>
public class StartupTasksMiddleware
{
	private readonly StartupTaskContext _context;
	private readonly RequestDelegate _next;

	/// <summary>
	///		The number of seconds to use for the Retry-After header when all startup tasks have not yet completed. Defaults to 30s.
	/// </summary>
	private const int RetryAfterSeconds = 10;

	/// <summary>
	///		The response code to return when all startup tasks have not yet completed. Defaults to 503 (Service Unavailable).
	/// </summary>
	private const int FailureResponseCode = (int)HttpStatusCode.ServiceUnavailable;

	public StartupTasksMiddleware(StartupTaskContext context, RequestDelegate next)
	{
		_context = context;
		_next = next;
	}

	public async Task Invoke(HttpContext httpContext)
	{
		// If the start-up tasks haven't finished, then give it extra 10 sec
		if (!_context.IsComplete)
		{
			const int maxAttempt = 20;
			var currentAttempt = 0;
			do
			{   // Wait 0.5 sec if start-up tasks haven't finished and there are no errors
				if (_context.IsComplete || _context.Errors?.Any() == true)
					break;
				await Task.Delay(500);
			} while (currentAttempt++ < maxAttempt);
		}

		if (_context.IsComplete)    // All Good
		{
			await _next(httpContext);
		}
		else if (_context.Errors?.Any() != true)    // Loading... 'Errors' is null or empty array
		{
			httpContext.Response.StatusCode = FailureResponseCode;
			httpContext.Response.Headers["Retry-After"] = RetryAfterSeconds.ToString(); // Not supported by Chrome and other browsers https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Retry-After
			string serialisedErrors = JsonConvert.SerializeObject(new[] { new { message = "Loading..." } });
			await httpContext.Response.WriteAsync(serialisedErrors);
		}
		else    // Failed start-up tasks
			throw new WebException("Errors on start-up: " + string.Join(',', _context.Errors));
	}
}