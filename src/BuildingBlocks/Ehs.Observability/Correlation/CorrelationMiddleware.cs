using Ehs.SharedKernel.Correlation;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Ehs.Observability.Correlation;

/// <summary>
/// Reads (or generates) the correlation/causation IDs for the incoming
/// request, stores them in <see cref="CorrelationContextAccessor"/>, echoes
/// the correlation ID back on the response, and pushes both onto the Serilog
/// LogContext so every log line for this request carries them (CLAUDE.md §7).
/// </summary>
public sealed class CorrelationMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, CorrelationContextAccessor correlationContextAccessor)
    {
        var correlationId = ReadOrCreateGuid(context, CorrelationHeaderNames.CorrelationId);
        var causationId = TryReadGuid(context, CorrelationHeaderNames.CausationId);

        correlationContextAccessor.Set(correlationId, causationId);
        context.Response.Headers[CorrelationHeaderNames.CorrelationId] = correlationId.ToString();

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("CausationId", causationId))
        {
            await _next(context);
        }
    }

    private static Guid ReadOrCreateGuid(HttpContext context, string headerName) =>
        TryReadGuid(context, headerName) ?? Guid.NewGuid();

    private static Guid? TryReadGuid(HttpContext context, string headerName)
    {
        var value = context.Request.Headers[headerName].FirstOrDefault();
        return Guid.TryParse(value, out var parsed) ? parsed : null;
    }
}
