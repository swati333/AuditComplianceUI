using System.Net;
using FluentAssertions;

namespace Finding.IntegrationTests;

public sealed class HealthCheckApiTests : IntegrationTestBase
{
    public HealthCheckApiTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Live_endpoint_reports_healthy()
    {
        var response = await Client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_endpoint_reports_healthy_when_the_database_is_reachable()
    {
        var response = await Client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
