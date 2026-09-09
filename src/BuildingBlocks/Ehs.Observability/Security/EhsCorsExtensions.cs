using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ehs.Observability.Security;

/// <summary>
/// Cross-origin policy for the React/Vite frontend, identically for every service.
/// Allowed origins come only from <c>Cors:AllowedOrigins</c> config (per-environment
/// appsettings) — never a wildcard — because auth is bearer-token based (no cookies),
/// so <see cref="Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicyBuilder.AllowCredentials"/>
/// is intentionally not enabled.
/// </summary>
public static class EhsCorsExtensions
{
    public const string PolicyName = "EhsFrontend";

    public static IServiceCollection AddEhsCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseEhsCors(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}
