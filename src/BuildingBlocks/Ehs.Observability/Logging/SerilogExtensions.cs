using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Formatting.Compact;

namespace Ehs.Observability.Logging;

/// <summary>
/// Structured logging convention shared by every service: compact JSON to
/// console (so container log collectors/Application Insights can parse
/// fields), enriched with service name, machine and environment. Per-request
/// CorrelationId/CausationId are pushed separately by <c>CorrelationMiddleware</c>.
/// </summary>
public static class SerilogExtensions
{
    public static WebApplicationBuilder UseEhsSerilog(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Service", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(new CompactJsonFormatter()));

        return builder;
    }
}
