using Audit.Api.Filters;
using Audit.Api.Services;
using Audit.Application.Common;
using Audit.Application.Services;
using Audit.Application.Validators;
using Audit.Infrastructure;
using Audit.Infrastructure.Persistence;
using Ehs.Observability.Correlation;
using Ehs.Observability.ExceptionHandling;
using Ehs.Observability.HealthChecks;
using Ehs.Observability.Logging;
using Ehs.Observability.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.UseEhsSerilog("AuditService");

// --- Building blocks (Ehs.Observability) ---
builder.Services.AddEhsCorrelation(); // AuditDbContext depends on ICorrelationContextAccessor
builder.Services.AddEhsExceptionHandling();
builder.Services.AddEhsEntraIdAuthentication(builder.Configuration);
builder.Services.AddEhsCors(builder.Configuration);

// --- Application / Infrastructure (this service) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAuditRequestValidator>();
builder.Services.AddAuditInfrastructure(builder.Configuration);
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<ChecklistService>();

builder.Services
    .AddEhsHealthChecks()
    .AddDbContextCheck<AuditDbContext>("audit-db", tags: [HealthCheckExtensions.ReadyTag]);

// --- Web / API ---
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EHS Audit Service",
        Version = "v1",
        Description = "Audit planning, execution, checklist and lifecycle management for the EHS Audit & Compliance platform.",
    });
    options.AddEhsEntraIdOAuth(builder.Configuration);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseEhsCorrelation();
app.UseEhsCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EHS Audit Service v1");
        options.UseEhsEntraIdOAuth(builder.Configuration);
    });

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
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
