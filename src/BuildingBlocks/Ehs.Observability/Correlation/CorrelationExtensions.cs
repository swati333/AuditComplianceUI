using Ehs.SharedKernel.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ehs.Observability.Correlation;

public static class CorrelationExtensions
{
    public static IServiceCollection AddEhsCorrelation(this IServiceCollection services)
    {
        services.AddSingleton<CorrelationContextAccessor>();
        services.AddSingleton<ICorrelationContextAccessor>(sp => sp.GetRequiredService<CorrelationContextAccessor>());
        return services;
    }

    public static IApplicationBuilder UseEhsCorrelation(this IApplicationBuilder app) =>
        app.UseMiddleware<CorrelationMiddleware>();
}
