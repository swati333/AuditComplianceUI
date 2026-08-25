using Ehs.Observability.Correlation;
using Ehs.SharedKernel.Correlation;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Correlation;

public class CorrelationMiddlewareTests
{
    // The accessor's backing store is AsyncLocal: values set inside InvokeAsync
    // only flow *downward* to code it calls (correctly modelling how a real
    // request pipeline sees them in controllers/services further down the
    // chain) and do not flow back up to the test once `await InvokeAsync(...)`
    // returns. So assertions on the accessor must run from inside `_next`.

    [Fact]
    public async Task Generates_a_correlation_id_when_no_header_is_present()
    {
        var context = new DefaultHttpContext();
        var accessor = new CorrelationContextAccessor();
        var observedCorrelationId = Guid.Empty;
        var nextCalled = false;

        var middleware = new CorrelationMiddleware(_ =>
        {
            nextCalled = true;
            observedCorrelationId = accessor.CorrelationId;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, accessor);

        Assert.True(nextCalled);
        Assert.NotEqual(Guid.Empty, observedCorrelationId);
        Assert.True(context.Response.Headers.ContainsKey(CorrelationHeaderNames.CorrelationId));
        Assert.Equal(observedCorrelationId.ToString(), context.Response.Headers[CorrelationHeaderNames.CorrelationId]);
    }

    [Fact]
    public async Task Reuses_the_incoming_correlation_id_header()
    {
        var incomingCorrelationId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationHeaderNames.CorrelationId] = incomingCorrelationId.ToString();
        var accessor = new CorrelationContextAccessor();
        var observedCorrelationId = Guid.Empty;

        var middleware = new CorrelationMiddleware(_ =>
        {
            observedCorrelationId = accessor.CorrelationId;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, accessor);

        Assert.Equal(incomingCorrelationId, observedCorrelationId);
        Assert.Equal(incomingCorrelationId.ToString(), context.Response.Headers[CorrelationHeaderNames.CorrelationId]);
    }

    [Fact]
    public async Task Reads_the_incoming_causation_id_header_when_present()
    {
        var causationId = Guid.NewGuid();
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationHeaderNames.CausationId] = causationId.ToString();
        var accessor = new CorrelationContextAccessor();
        Guid? observedCausationId = null;

        var middleware = new CorrelationMiddleware(_ =>
        {
            observedCausationId = accessor.CausationId;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, accessor);

        Assert.Equal(causationId, observedCausationId);
    }

    [Fact]
    public async Task CausationId_is_null_when_header_is_absent_or_malformed()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationHeaderNames.CausationId] = "not-a-guid";
        var accessor = new CorrelationContextAccessor();
        Guid? observedCausationId = Guid.NewGuid();

        var middleware = new CorrelationMiddleware(_ =>
        {
            observedCausationId = accessor.CausationId;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(context, accessor);

        Assert.Null(observedCausationId);
    }
}
