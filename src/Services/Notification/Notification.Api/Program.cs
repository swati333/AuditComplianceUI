using Ehs.Observability.Correlation;
using Ehs.Observability.ExceptionHandling;
using Ehs.Observability.HealthChecks;
using Ehs.Observability.Logging;
using Ehs.Observability.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Notification.Api.Filters;
using Notification.Api.Services;
using Notification.Application.Common;
using Notification.Application.Services;
using Notification.Infrastructure;
using Notification.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.UseEhsSerilog("NotificationService");

// --- Building blocks (Ehs.Observability) ---
builder.Services.AddEhsCorrelation();
builder.Services.AddEhsExceptionHandling();
builder.Services.AddEhsEntraIdAuthentication(builder.Configuration);

// --- Application / Infrastructure (this service) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddValidatorsFromAssemblyContaining<NotificationFactory>();
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddScoped<NotificationQueryService>();
builder.Services.AddScoped<NotificationPreferenceService>();
builder.Services.AddScoped<NotificationTemplateQueryService>();

builder.Services
    .AddEhsHealthChecks()
    .AddDbContextCheck<NotificationDbContext>("notification-db", tags: [HealthCheckExtensions.ReadyTag]);

// --- Web / API ---
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EHS Notification Service",
        Version = "v1",
        Description = "In-app and email notifications, templates and per-user preferences for the EHS Audit & Compliance platform.",
    });
    options.AddEhsEntraIdOAuth(builder.Configuration);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseEhsCorrelation();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EHS Notification Service v1");
        options.UseEhsEntraIdOAuth(builder.Configuration);
    });

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.MigrateAsync();
    await SeedData.SeedAsync(dbContext);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseEhsUserContextLogging();

app.MapControllers();
app.MapEhsHealthChecks();

app.Run();

/// <summary>Exposed so WebApplicationFactory&lt;Program&gt; can bootstrap this app in integration tests.</summary>
public partial class Program
{
}
