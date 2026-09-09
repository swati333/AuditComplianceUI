using ActionPlan.Api.Filters;
using ActionPlan.Api.Services;
using ActionPlan.Application.Common;
using ActionPlan.Application.Services;
using ActionPlan.Application.Validators;
using ActionPlan.Infrastructure;
using ActionPlan.Infrastructure.Persistence;
using Ehs.Observability.Correlation;
using Ehs.Observability.ExceptionHandling;
using Ehs.Observability.HealthChecks;
using Ehs.Observability.Logging;
using Ehs.Observability.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.UseEhsSerilog("ActionPlanService");

// --- Building blocks (Ehs.Observability) ---
builder.Services.AddEhsCorrelation(); // ActionPlanDbContext depends on ICorrelationContextAccessor
builder.Services.AddEhsExceptionHandling();
builder.Services.AddEhsEntraIdAuthentication(builder.Configuration);
builder.Services.AddEhsCors(builder.Configuration);

// --- Application / Infrastructure (this service) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateActionPlanRequestValidator>();
builder.Services.AddActionPlanInfrastructure(builder.Configuration);
builder.Services.AddScoped<ActionPlanService>();

builder.Services
    .AddEhsHealthChecks()
    .AddDbContextCheck<ActionPlanDbContext>("actionplan-db", tags: [HealthCheckExtensions.ReadyTag]);

// --- Web / API ---
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EHS Action Plan Service",
        Version = "v1",
        Description = "Corrective/preventive action assignment, approval and closure lifecycle for the EHS Audit & Compliance platform.",
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EHS Action Plan Service v1");
        options.UseEhsEntraIdOAuth(builder.Configuration);
    });

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ActionPlanDbContext>();
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
