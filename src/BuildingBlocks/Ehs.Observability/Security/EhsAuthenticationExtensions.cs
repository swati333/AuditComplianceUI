using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace Ehs.Observability.Security;

/// <summary>
/// Wires Microsoft Entra ID / JWT bearer authentication and the 7 policies
/// required by CLAUDE.md §10, identically for every service. Issuer,
/// audience and signing-key validation come from Entra's OIDC metadata
/// (resolved lazily from <c>AzureAd:TenantId</c>/<c>ClientId</c> on first
/// token validation — never at startup, so a placeholder config value never
/// prevents the service from starting); lifetime validation is
/// <see cref="Microsoft.IdentityModel.Tokens.TokenValidationParameters"/>'s
/// default and is restated explicitly below so it's not just an implicit
/// library default.
/// </summary>
public static class EhsAuthenticationExtensions
{
    public static IServiceCollection AddEhsEntraIdAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidateAudience = true;
            options.TokenValidationParameters.ValidateLifetime = true;
            options.TokenValidationParameters.ValidateIssuerSigningKey = true;
            options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(2);
        });

        services.AddAuthorizationBuilder()
            .AddPolicy(EhsAuthorizationPolicies.CanManageAudits, policy => policy.RequireRole(EntraAppRoles.AuditManage))
            .AddPolicy(EhsAuthorizationPolicies.CanPerformAudits, policy => policy.RequireRole(EntraAppRoles.AuditPerform))
            .AddPolicy(EhsAuthorizationPolicies.CanManageFindings, policy => policy.RequireRole(EntraAppRoles.FindingManage))
            .AddPolicy(EhsAuthorizationPolicies.CanManageOwnActions, policy => policy.RequireRole(EntraAppRoles.ActionManageOwn))
            .AddPolicy(EhsAuthorizationPolicies.CanApproveActions, policy => policy.RequireRole(EntraAppRoles.ActionApprove))
            .AddPolicy(EhsAuthorizationPolicies.CanViewReports, policy => policy.RequireRole(EntraAppRoles.ReportView))
            .AddPolicy(EhsAuthorizationPolicies.CanManageConfiguration, policy => policy.RequireRole(EntraAppRoles.ConfigurationManage));

        return services;
    }
}
