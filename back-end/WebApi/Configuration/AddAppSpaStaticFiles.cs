using System;
using System.Linq;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

using Raven.Yabt.WebApi.Configuration.Settings;

namespace Raven.Yabt.WebApi.Configuration
{
	internal static partial class ServiceCollectionExtensions
	{
		private const string CookieNameApiBaseUrl = "apiBaseUrl";
		private const string CookieNameApiUserKeys = "apiUserKeys";
		
		/// <summary>
		///		Register hosting of the SPA app including:
		///			- caching policy for static content
		///			- base URL for the back-end
		///			- the API key (forgive me, it's just an educational project) 
		/// </summary>
		/// <remarks>
		///		Links for 'UseStaticFiles' vs 'UseSpa' - https://stackoverflow.com/a/56977859/968003
		/// </remarks>
		public static void AddAppSpaStaticFiles(this IApplicationBuilder app, AppSettingsUserApiKey[] userApiKeys)
		{
			// Serve files inside of web root (wwwroot folder) other than 'index.html'
			// Without this method Kestrel would return 'index.html' on all the requests for static content 
			app.UseStaticFiles(
				// Set cache expiration for all static files except 'index.html'
				new StaticFileOptions
				{
					OnPrepareResponse = ctx =>
					{
						var headers = ctx.Context.Response.GetTypedHeaders();
						headers.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromDays(12*30) };
					}
				});

			var apiKeys = string.Join(';', userApiKeys.Select(k => k.ApiKey));

			// Does 3 things:
			//	- Rewrites all requests to the default page;
			//	- Serves 'index.html'
			//	- Tries to configure static files serving (falls back to UseSpaStaticFiles() and serving them from 'wwwroot')
			app.UseSpa(
				// Disable cache for 'index.html' (https://stackoverflow.com/q/49547/968003)
				// The correct minimum set includes:
				//     Cache-Control: no-cache, no-store, must-revalidate, max-age=0
				c => c.Options.DefaultPageStaticFileOptions = new StaticFileOptions
				{
					OnPrepareResponse = ctx =>
					{
						var response = ctx.Context.Response; 
						response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue	// Supersedes 'Pragma' header (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Pragma)
						{
							NoCache = true,
							NoStore = true,
							MustRevalidate = true,
							MaxAge = TimeSpan.Zero	// Overtakes 'Expires: 0' (https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Expires)
						};
						// The front-end is immutable, so it doesn't change on deployment.
						// Two options to pass the API URL into the SPA:
						//	a) inject into 'index.html' (would need add an MVC controller to serve altered 'index.html')
						//	b) pass into cookie (it's simple, see below) 
						response.Cookies.Append(CookieNameApiBaseUrl, "/", new CookieOptions { MaxAge = TimeSpan.FromSeconds(30) });
						response.Cookies.Append(CookieNameApiUserKeys, apiKeys, new CookieOptions { MaxAge = TimeSpan.FromSeconds(30) });
					}
				});
		}
	}
}
