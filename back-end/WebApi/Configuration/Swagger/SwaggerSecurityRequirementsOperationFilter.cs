using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.OpenApi.Models;

using Raven.Yabt.WebApi.Authorization.ApiKeyAuth;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Raven.Yabt.WebApi.Configuration.Swagger;

/// <summary>
///     Add the authenticated information to each operation
///     Also allows the user to login from the operation if they aren't already logged in
/// </summary>
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class SwaggerSecurityRequirementsOperationFilter : IOperationFilter
{
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		// All the end-points are protected
		operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
		operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
			
		// Setup the API key scheme as the default authentication scheme
		var authScheme = new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = PredefinedUserApiKeyAuthOptions.DefaultScheme
				},
				Scheme = PredefinedUserApiKeyAuthOptions.DefaultScheme,
				Name = PredefinedUserApiKeyAuthHandler.ApiKeyHeaderName,
				Type = SecuritySchemeType.ApiKey,
				In = ParameterLocation.Header
			};
		operation.Security = new List<OpenApiSecurityRequirement>
			{
				new() { [ authScheme ] = System.Array.Empty<string>() }
			};
	}
}