using ActionPlan.Application.Common;
using ActionPlan.Application.EventHandlers;
using ActionPlan.Infrastructure.Messaging;
using ActionPlan.Infrastructure.Persistence;
using ActionPlan.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ActionPlan.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers everything this service's Infrastructure layer owns. The
    /// caller (ActionPlan.Api's Program.cs) must separately call
    /// <c>Ehs.Observability.Correlation.CorrelationExtensions.AddEhsCorrelation()</c>
    /// — <see cref="Persistence.ActionPlanDbContext"/> depends on
    /// <c>ICorrelationContextAccessor</c>, and the Api layer needs that same
    /// registration for its correlation middleware anyway.
    /// </summary>
    public static IServiceCollection AddActionPlanInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ActionPlanDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'ActionPlanDb' is not configured.");
        }

        services.AddDbContext<ActionPlanDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ActionPlanDbContext).Assembly.FullName)));

        services.AddScoped<IActionPlanRepository, ActionPlanRepository>();
        services.AddScoped<IFindingReferenceRepository, FindingReferenceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IntegrationEventConsumer>();
        services.AddScoped<FindingCreatedHandler>();
        services.AddScoped<FindingClosedHandler>();

        services.AddSingleton<IOutboxEventPublisher, LoggingOutboxEventPublisher>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHostedService<OverdueDetectionService>();

        return services;
    }
}
