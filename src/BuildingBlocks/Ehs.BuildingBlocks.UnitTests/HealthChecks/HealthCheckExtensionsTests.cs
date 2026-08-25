using Ehs.Observability.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.HealthChecks;

public class HealthCheckExtensionsTests
{
    [Fact]
    public void AddEhsHealthChecks_registers_a_resolvable_HealthCheckService()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddEhsHealthChecks();
        using var provider = services.BuildServiceProvider();

        var healthCheckService = provider.GetService<HealthCheckService>();

        Assert.NotNull(healthCheckService);
    }

    [Fact]
    public async Task Self_check_is_tagged_live_and_ready_and_reports_healthy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddEhsHealthChecks();
        using var provider = services.BuildServiceProvider();

        var report = await provider.GetRequiredService<HealthCheckService>().CheckHealthAsync();

        var selfEntry = Assert.Single(report.Entries, e => e.Key == "self").Value;
        Assert.Equal(HealthStatus.Healthy, selfEntry.Status);
        Assert.Contains(HealthCheckExtensions.LiveTag, selfEntry.Tags);
        Assert.Contains(HealthCheckExtensions.ReadyTag, selfEntry.Tags);
    }
}
