using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Raven.Yabt.WebApi.Authorization;

/// <summary>
///     A custom authorize filter that can be applied globally but does nothing if the targeted method
///     has another authorize attribute on it.
/// </summary>
/// <inheritdoc />
public class GlobalAuthorizeFilter : AuthorizeFilter
{
	public GlobalAuthorizeFilter(AuthorizationPolicy policy) : base(policy) {}

	public override Task OnAuthorizationAsync(AuthorizationFilterContext context)
	{
		// If there is another authorize filter, do nothing
		return context.Filters.Any(item => item is IAsyncAuthorizationFilter && item != this)
			? Task.CompletedTask
			: base.OnAuthorizationAsync(context);
	}
}