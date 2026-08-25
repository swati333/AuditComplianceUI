using Microsoft.Extensions.DependencyInjection;

namespace Ehs.Observability.ExceptionHandling;

public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Registers the global exception handler plus ASP.NET Core's built-in
    /// ProblemDetails middleware (for framework-originated 4xx/5xx responses,
    /// e.g. [ApiController] model-validation failures) so every error path
    /// through the API produces RFC 7807 output.
    /// </summary>
    public static IServiceCollection AddEhsExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<EhsExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
