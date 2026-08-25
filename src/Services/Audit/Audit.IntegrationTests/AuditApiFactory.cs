using Audit.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.MsSql;

namespace Audit.IntegrationTests;

/// <summary>
/// Boots the real Api pipeline (Program.cs — middleware, DI, controllers)
/// against a real SQL Server running in a Testcontainers container
/// (CLAUDE.md §3/§11 — no mocked database). The container's connection
/// string replaces whatever Audit.Api's own appsettings.json has: Program.cs
/// still runs its normal `AddAuditInfrastructure` registration (so the guard
/// against a missing connection string is exercised too), but
/// <see cref="ConfigureWebHost"/> swaps the resulting DbContextOptions for
/// the container before the host is built — the standard, Microsoft-documented
/// pattern for testing an app that owns its own DbContext registration.
/// </summary>
public sealed class AuditApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        // Force host creation now (not on first HTTP call) so migrations are
        // applied before any test issues a request.
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // "Testing" (not "Development"): skip Program.cs's own auto-migrate/
        // seed/Swagger block — this factory controls migration explicitly,
        // and tests build their own fixtures rather than relying on seed data.
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AuditDbContext>>();
            services.AddDbContext<AuditDbContext>(options => options.UseSqlServer(_sqlContainer.GetConnectionString()));

            // Overrides Program.cs's real Entra ID JWT bearer scheme as the
            // default — see TestAuthHandler for why.
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
