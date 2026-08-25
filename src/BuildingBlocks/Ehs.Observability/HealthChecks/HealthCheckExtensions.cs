using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ehs.Observability.HealthChecks;

/// <summary>
/// Standard <c>/health/live</c> and <c>/health/ready</c> convention every
/// service must expose (CLAUDE.md §4). "Live" only proves the process is up
/// (the "self" check); "ready" runs every registered check, so each service
/// adds its own dependency checks (DB, Service Bus, Blob Storage) tagged
/// "ready" via the standard <see cref="IHealthChecksBuilder"/> this returns.
/// </summary>
public static class HealthCheckExtensions
{
    public const string LiveTag = "live";
    public const string ReadyTag = "ready";

    public static IHealthChecksBuilder AddEhsHealthChecks(this IServiceCollection services) =>
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), [LiveTag, ReadyTag]);

    public static IEndpointRouteBuilder MapEhsHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(LiveTag),
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag),
        });

        return endpoints;
    }
}
