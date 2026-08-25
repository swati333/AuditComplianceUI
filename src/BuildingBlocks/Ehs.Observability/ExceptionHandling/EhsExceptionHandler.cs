using Ehs.SharedKernel.Correlation;
using Ehs.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ehs.Observability.ExceptionHandling;

/// <summary>
/// Global exception handler (ASP.NET Core <see cref="IExceptionHandler"/>)
/// that turns any unhandled exception into the RFC 7807 ProblemDetails shape
/// mandated by CLAUDE.md §9, including a stable <c>errorCode</c> and the
/// request's <c>traceId</c>. Never leaks stack traces/internal details.
/// </summary>
public sealed class EhsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<EhsExceptionHandler> _logger;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public EhsExceptionHandler(ILogger<EhsExceptionHandler> logger, ICorrelationContextAccessor correlationContextAccessor)
    {
        _logger = logger;
        _correlationContextAccessor = correlationContextAccessor;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, title, detail) = Classify(exception);

        _logger.LogError(
            exception,
            "Unhandled exception. ErrorCode={ErrorCode} StatusCode={StatusCode} TraceId={TraceId} CorrelationId={CorrelationId}",
            errorCode,
            statusCode,
            httpContext.TraceIdentifier,
            _correlationContextAccessor.CorrelationId);

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.io/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };
        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is BusinessValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors;
        }

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    /// <summary>
    /// Detail text is only ever taken from <see cref="AppException"/>.Message,
    /// because those messages are hand-authored by our own Application/Domain
    /// code to be safe for clients. Any other exception (SQL errors, null
    /// refs, etc.) gets a fixed generic detail — its real message/stack trace
    /// goes to the log above, never to the response body.
    /// </summary>
    private static (int StatusCode, string ErrorCode, string Title, string Detail) Classify(Exception exception) => exception switch
    {
        NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.ErrorCode, "Resource not found", notFound.Message),
        ConflictException conflict => (StatusCodes.Status409Conflict, conflict.ErrorCode, "Conflict", conflict.Message),
        ForbiddenException forbidden => (StatusCodes.Status403Forbidden, forbidden.ErrorCode, "Forbidden", forbidden.Message),
        BusinessValidationException validation => (StatusCodes.Status400BadRequest, validation.ErrorCode, "Validation error", validation.Message),
        AppException appException => (StatusCodes.Status400BadRequest, appException.ErrorCode, "Application error", appException.Message),
        _ => (StatusCodes.Status500InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred", "An unexpected error occurred. Contact support with the trace ID if the problem persists."),
    };
}
