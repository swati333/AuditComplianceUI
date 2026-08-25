using Ehs.Observability.Logging;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Logging;

public class SerilogExtensionsTests
{
    [Fact]
    public void UseEhsSerilog_configures_the_host_without_throwing()
    {
        var builder = WebApplication.CreateBuilder();

        builder.UseEhsSerilog("Ehs.Tests.SampleService");

        using var app = builder.Build();

        // Building the host is enough to prove the Serilog logging provider
        // wiring (UseSerilog + enrichers + console sink) is well-formed.
        Assert.NotNull(app.Services.GetService(typeof(Microsoft.Extensions.Logging.ILoggerFactory)));
    }
}
