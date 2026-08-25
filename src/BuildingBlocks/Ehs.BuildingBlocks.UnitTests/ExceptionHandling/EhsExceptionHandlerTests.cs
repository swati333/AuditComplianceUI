using System.Text.Json;
using Ehs.Observability.Correlation;
using Ehs.Observability.ExceptionHandling;
using Ehs.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.ExceptionHandling;

public class EhsExceptionHandlerTests
{
    private readonly EhsExceptionHandler _handler = new(
        NullLogger<EhsExceptionHandler>.Instance,
        new CorrelationContextAccessor());

    [Fact]
    public async Task NotFoundException_maps_to_404_with_its_error_code()
    {
        var (statusCode, body) = await InvokeAsync(NotFoundException.For("Audit", "123"));

        Assert.Equal(StatusCodes.Status404NotFound, statusCode);
        Assert.Equal("RESOURCE_NOT_FOUND", body.GetProperty("errorCode").GetString());
        Assert.Equal("Audit '123' was not found.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task BusinessValidationException_includes_field_errors_extension()
    {
        var errors = new Dictionary<string, string[]> { ["Scope"] = ["Scope is required."] };

        var (statusCode, body) = await InvokeAsync(new BusinessValidationException(errors));

        Assert.Equal(StatusCodes.Status400BadRequest, statusCode);
        Assert.Equal("VALIDATION_ERROR", body.GetProperty("errorCode").GetString());
        Assert.Equal("Scope is required.", body.GetProperty("errors").GetProperty("Scope")[0].GetString());
    }

    [Fact]
    public async Task ConflictException_maps_to_409()
    {
        var (statusCode, body) = await InvokeAsync(new ConflictException("Audit cannot close while findings are open."));

        Assert.Equal(StatusCodes.Status409Conflict, statusCode);
        Assert.Equal("RESOURCE_CONFLICT", body.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Unknown_exception_maps_to_500_without_leaking_the_original_message()
    {
        var (statusCode, body) = await InvokeAsync(new InvalidOperationException("connection string: Server=prod-sql;Password=hunter2"));

        Assert.Equal(StatusCodes.Status500InternalServerError, statusCode);
        Assert.Equal("INTERNAL_SERVER_ERROR", body.GetProperty("errorCode").GetString());
        Assert.DoesNotContain("hunter2", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Response_always_carries_the_request_trace_id()
    {
        var (_, body) = await InvokeAsync(new NotFoundException("x"));

        Assert.False(string.IsNullOrEmpty(body.GetProperty("traceId").GetString()));
    }

    private async Task<(int StatusCode, JsonElement Body)> InvokeAsync(Exception exception)
    {
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() },
            // WriteAsJsonAsync resolves IOptions<JsonOptions> from RequestServices;
            // an empty provider makes it fall back to default JsonOptions instead of NPE-ing.
            RequestServices = new ServiceCollection().BuildServiceProvider(),
        };

        var handled = await _handler.TryHandleAsync(context, exception, CancellationToken.None);
        Assert.True(handled);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var json = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, JsonDocument.Parse(json).RootElement);
    }
}
