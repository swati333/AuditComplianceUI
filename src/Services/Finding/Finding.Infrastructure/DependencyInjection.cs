using Finding.Application.Common;
using Finding.Application.EventHandlers;
using Finding.Infrastructure.Messaging;
using Finding.Infrastructure.Persistence;
using Finding.Infrastructure.Persistence.Repositories;
using Finding.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Finding.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers everything this service's Infrastructure layer owns. The
    /// caller (Finding.Api's Program.cs) must separately call
    /// <c>Ehs.Observability.Correlation.CorrelationExtensions.AddEhsCorrelation()</c>
    /// — <see cref="Persistence.FindingDbContext"/> depends on
    /// <c>ICorrelationContextAccessor</c>, and the Api layer needs that same
    /// registration for its correlation middleware anyway.
    /// </summary>
    public static IServiceCollection AddFindingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FindingDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'FindingDb' is not configured.");
        }

        services.AddDbContext<FindingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(FindingDbContext).Assembly.FullName)));

        services.AddScoped<IFindingRepository, FindingRepository>();
        services.AddScoped<IAuditReferenceRepository, AuditReferenceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IActionPlanGateway, ActionPlanGateway>();

        services.AddScoped<IntegrationEventConsumer>();
        services.AddScoped<AuditCreatedHandler>();
        services.AddScoped<AuditClosedHandler>();

        services.AddSingleton<IOutboxEventPublisher, LoggingOutboxEventPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
