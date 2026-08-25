using Audit.Application.Common;
using Audit.Infrastructure.Messaging;
using Audit.Infrastructure.Persistence;
using Audit.Infrastructure.Persistence.Repositories;
using Audit.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers everything this service's Infrastructure layer owns. The
    /// caller (Audit.Api's Program.cs) must separately call
    /// <c>Ehs.Observability.Correlation.CorrelationExtensions.AddEhsCorrelation()</c>
    /// — <see cref="Persistence.AuditDbContext"/> depends on
    /// <c>ICorrelationContextAccessor</c>, and the Api layer needs that same
    /// registration for its correlation middleware anyway, so it isn't
    /// duplicated here.
    /// </summary>
    public static IServiceCollection AddAuditInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AuditDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'AuditDb' is not configured.");
        }

        services.AddDbContext<AuditDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AuditDbContext).Assembly.FullName)));

        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IChecklistRepository, ChecklistRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditComplianceGateway, AuditComplianceGateway>();

        services.AddSingleton<IOutboxEventPublisher, LoggingOutboxEventPublisher>();
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}
