using Finding.Api.Filters;
using Finding.Api.Services;
using Finding.Application.Common;
using Finding.Application.Services;
using Finding.Application.Validators;
using Finding.Infrastructure;
using Finding.Infrastructure.Persistence;
using Ehs.Observability.Correlation;
using Ehs.Observability.ExceptionHandling;
using Ehs.Observability.HealthChecks;
using Ehs.Observability.Logging;
using Ehs.Observability.Security;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.UseEhsSerilog("FindingService");

// --- Building blocks (Ehs.Observability) ---
builder.Services.AddEhsCorrelation(); // FindingDbContext depends on ICorrelationContextAccessor
builder.Services.AddEhsExceptionHandling();
builder.Services.AddEhsEntraIdAuthentication(builder.Configuration);

// --- Application / Infrastructure (this service) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateFindingRequestValidator>();
builder.Services.AddFindingInfrastructure(builder.Configuration);
builder.Services.AddScoped<FindingService>();

builder.Services
    .AddEhsHealthChecks()
    .AddDbContextCheck<FindingDbContext>("finding-db", tags: [HealthCheckExtensions.ReadyTag]);

// --- Web / API ---
builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EHS Finding Service",
        Version = "v1",
        Description = "Compliance-finding lifecycle, root-cause analysis, comments and document metadata for the EHS Audit & Compliance platform.",
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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "EHS Finding Service v1");
        options.UseEhsEntraIdOAuth(builder.Configuration);
    });

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<FindingDbContext>();
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
