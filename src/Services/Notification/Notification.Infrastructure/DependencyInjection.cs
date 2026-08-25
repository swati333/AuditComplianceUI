using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Common;
using Notification.Application.EventHandlers;
using Notification.Application.Services;
using Notification.Infrastructure.Email;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;

namespace Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NotificationDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'NotificationDb' is not configured.");
        }

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<NotificationFactory>();
        services.AddScoped<IntegrationEventConsumer>();
        services.AddScoped<AuditPlannedHandler>();
        services.AddScoped<CriticalFindingCreatedHandler>();
        services.AddScoped<ActionPlanAssignedHandler>();
        services.AddScoped<ActionPlanSubmittedHandler>();
        services.AddScoped<ActionPlanRejectedHandler>();
        services.AddScoped<ActionPlanOverdueHandler>();
        services.AddScoped<ReportGeneratedHandler>();
        services.AddScoped<ReportGenerationFailedHandler>();

        services.Configure<FakeLocalEmailProviderOptions>(configuration.GetSection(FakeLocalEmailProviderOptions.SectionName));
        services.AddSingleton<IEmailProvider, FakeLocalEmailProvider>();
        services.AddHostedService<NotificationDispatcher>();

        return services;
    }
}
