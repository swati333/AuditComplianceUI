using Ehs.Observability.Correlation;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Correlation;

public class CorrelationContextAccessorTests
{
    [Fact]
    public void Before_Set_returns_empty_correlation_id_and_null_causation_id()
    {
        // The backing store is a static AsyncLocal, so it must be observed on a
        // brand-new OS thread: a raw System.Threading.Thread does not flow the
        // parent's ExecutionContext, unlike Task/ThreadPool. Using the xUnit
        // thread directly here would be order-dependent on whatever earlier
        // test last called Set() on this same thread.
        var accessor = new CorrelationContextAccessor();
        var observedCorrelationId = Guid.Empty;
        Guid? observedCausationId = Guid.NewGuid();

        var thread = new Thread(() =>
        {
            observedCorrelationId = accessor.CorrelationId;
            observedCausationId = accessor.CausationId;
        });
        thread.Start();
        thread.Join();

        Assert.Equal(Guid.Empty, observedCorrelationId);
        Assert.Null(observedCausationId);
    }

    [Fact]
    public void Set_makes_the_ids_readable_on_the_same_accessor_instance()
    {
        var accessor = new CorrelationContextAccessor();
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();

        accessor.Set(correlationId, causationId);

        Assert.Equal(correlationId, accessor.CorrelationId);
        Assert.Equal(causationId, accessor.CausationId);
    }

    [Fact]
    public async Task Values_are_isolated_per_async_flow()
    {
        var accessor = new CorrelationContextAccessor();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();

        var firstObserved = Guid.Empty;
        var secondObserved = Guid.Empty;

        var firstFlow = Task.Run(async () =>
        {
            accessor.Set(firstId, null);
            await Task.Delay(50);
            firstObserved = accessor.CorrelationId;
        });

        var secondFlow = Task.Run(async () =>
        {
            accessor.Set(secondId, null);
            await Task.Delay(50);
            secondObserved = accessor.CorrelationId;
        });

        await Task.WhenAll(firstFlow, secondFlow);

        Assert.Equal(firstId, firstObserved);
        Assert.Equal(secondId, secondObserved);
    }
}
