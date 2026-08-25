using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace Notification.IntegrationTests;

/// <summary>
/// Boots the real Api pipeline against a real SQL Server running in
/// Testcontainers — see Audit/Finding Service's identical factories for the
/// full rationale. Also redirects the fake email provider's output to a
/// per-run temp directory so tests can inspect written .eml files without
/// touching the repo's real App_Data folder.
/// </summary>
public sealed class NotificationApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public string FakeEmailOutputDirectory { get; } = Path.Combine(Path.GetTempPath(), $"notification-tests-{Guid.NewGuid():N}");

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        await dbContext.Database.MigrateAsync();
        await SeedData.SeedAsync(dbContext);
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
        await base.DisposeAsync();

        if (Directory.Exists(FakeEmailOutputDirectory))
        {
            Directory.Delete(FakeEmailOutputDirectory, recursive: true);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FakeEmail:OutputDirectory"] = FakeEmailOutputDirectory,
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<NotificationDbContext>>();
            services.AddDbContext<NotificationDbContext>(options => options.UseSqlServer(_sqlContainer.GetConnectionString()));

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }
}
