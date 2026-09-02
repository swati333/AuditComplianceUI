using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Ehs.Observability.Security;

/// <summary>
/// Swagger OAuth2 wiring for Development only (CLAUDE.md §10/§6: "Swagger/
/// OpenAPI enabled with authentication support in development"). All values
/// come from <c>AzureAd</c> config, which ships with placeholders — see
/// docs/entra-id-setup.md for what a real Entra App Registration needs to
/// supply.
/// </summary>
public static class EhsSwaggerAuthExtensions
{
    private const string SchemeName = "oauth2";

    public static void AddEhsEntraIdOAuth(this SwaggerGenOptions options, IConfiguration configuration)
    {
        var tenantId = configuration["AzureAd:TenantId"] ?? "a77d9ea3 - d8ed - 44a3-b296-0d1ca26ce894";
        var scope = configuration["AzureAd:ApiScope"] ?? "api://ehsmicro.onmicrosoft.com/auditcompliance";

        options.AddSecurityDefinition(SchemeName, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                    TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        [scope] = "Access the API as the signed-in user",
                    },
                },
            },
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = SchemeName } }] = new[] { scope },
        });
    }

    public static void UseEhsEntraIdOAuth(this SwaggerUIOptions options, IConfiguration configuration)
    {
        options.OAuthClientId(configuration["AzureAd:SwaggerClientId"] ?? "REPLACE_WITH_SWAGGER_SPA_CLIENT_ID");
        options.OAuthUsePkce();
    }
}
