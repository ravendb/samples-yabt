using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Raven.Yabt.WebApi.Infrastructure.StartupTasks;

/// <summary>
///		Singleton object to keep track of whether all of the startup tasks have finished
/// </summary>
/// <remarks>
///		Based on https://andrewlock.net/running-async-tasks-on-app-startup-in-asp-net-core-part-4-using-health-checks/
/// </remarks>
public class StartupTaskContext
{
	private int _outstandingTaskCount;
	private readonly ConcurrentBag<string> _errors = new ConcurrentBag<string>();

	public void RegisterTask()
	{
		Interlocked.Increment(ref _outstandingTaskCount);
	}

	public void MarkTaskAsComplete()
	{
		Interlocked.Decrement(ref _outstandingTaskCount);
	}

	public void MarkTaskAsFailed(IList<string> errors)
	{
		if (errors.Any())
			foreach (var error in errors)
				_errors.Add(error);
		else
			_errors.Add("Unhandled error");
	}

	public bool IsComplete => _outstandingTaskCount == 0;

	public string[]? Errors => !IsComplete ? _errors.ToArray() : null;
}