using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace Raven.Yabt.WebApi.Configuration;

internal static partial class ServiceCollectionExtensions
{
	/// <summary>
	///		Register exception handler to return <see cref="ProblemDetails"/>
	/// </summary>
	/// <remarks>
	///		Based on the official recommendations - https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-5.0#exception-handler
	/// </remarks>
	public static void AddAppExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment env)
	{
		app.UseExceptionHandler(a 
			=> a.Run(async context =>
			{
				var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
				var problem = new ProblemDetails { Title = "Critical Error"};
				if (error != null)
				{
					if (env.IsDevelopment())
					{
						problem.Title = error.Message;
						problem.Detail = error.StackTrace;
					}
					else
						problem.Detail = error.Message;
				}
				await context.Response.WriteAsJsonAsync(problem);
			})
		);
	}
}