using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Ehs.Observability.Security;

/// <summary>
/// Pushes the caller's validated <c>oid</c>/<c>tid</c> claims onto Serilog's
/// LogContext, alongside the CorrelationId/CausationId
/// <c>CorrelationMiddleware</c> already pushes — CLAUDE.md §10: "structured
/// logs include trace/correlation/tenant/user IDs." Must run after
/// <c>UseAuthentication</c>/<c>UseAuthorization</c> (unlike
/// <c>CorrelationMiddleware</c>, which runs before them) since
/// <see cref="HttpContext.User"/> isn't populated until then.
/// </summary>
public sealed class UserContextLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirst("oid")?.Value;
        var tenantId = context.User.FindFirst("tid")?.Value;

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("TenantId", tenantId))
        {
            await _next(context);
        }
    }
}

public static class UserContextLoggingExtensions
{
    public static IApplicationBuilder UseEhsUserContextLogging(this IApplicationBuilder app) =>
        app.UseMiddleware<UserContextLoggingMiddleware>();
}
