using Ehs.SharedKernel.Domain;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Domain;

public class DomainEventTests
{
    private sealed record SampleDomainEvent : DomainEvent;

    [Fact]
    public void EventId_is_unique_per_instance()
    {
        var first = new SampleDomainEvent();
        var second = new SampleDomainEvent();

        Assert.NotEqual(first.EventId, second.EventId);
        Assert.NotEqual(Guid.Empty, first.EventId);
    }

    [Fact]
    public void OccurredOnUtc_is_set_at_construction_time()
    {
        var before = DateTime.UtcNow;
        var domainEvent = new SampleDomainEvent();
        var after = DateTime.UtcNow;

        Assert.InRange(domainEvent.OccurredOnUtc, before, after);
    }
}
