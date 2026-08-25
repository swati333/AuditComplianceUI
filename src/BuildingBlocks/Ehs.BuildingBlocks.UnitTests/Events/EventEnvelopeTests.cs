using Ehs.Contracts.Events;
using Xunit;

namespace Ehs.BuildingBlocks.UnitTests.Events;

public class EventEnvelopeTests
{
    private sealed record SamplePayload(string Message) : IIntegrationEvent;

    [Fact]
    public void Create_defaults_EventType_to_payload_type_name()
    {
        var envelope = EventEnvelope.Create(
            new SamplePayload("hello"),
            source: "TestService",
            correlationId: Guid.NewGuid());

        Assert.Equal(nameof(SamplePayload), envelope.EventType);
    }

    [Fact]
    public void Create_allows_overriding_EventType_and_EventVersion()
    {
        var envelope = EventEnvelope.Create(
            new SamplePayload("hello"),
            source: "TestService",
            correlationId: Guid.NewGuid(),
            eventType: "CustomEventType",
            eventVersion: 2);

        Assert.Equal("CustomEventType", envelope.EventType);
        Assert.Equal(2, envelope.EventVersion);
    }

    [Fact]
    public void Create_propagates_correlation_causation_tenant_and_user()
    {
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var envelope = EventEnvelope.Create(
            new SamplePayload("hello"),
            source: "AuditService",
            correlationId: correlationId,
            causationId: causationId,
            tenantId: tenantId,
            userId: "entra-object-id");

        Assert.Equal(correlationId, envelope.CorrelationId);
        Assert.Equal(causationId, envelope.CausationId);
        Assert.Equal(tenantId, envelope.TenantId);
        Assert.Equal("entra-object-id", envelope.UserId);
        Assert.Equal("AuditService", envelope.Source);
        Assert.Equal("hello", envelope.Payload.Message);
    }

    [Fact]
    public void EventId_and_OccurredOnUtc_are_populated_automatically()
    {
        var before = DateTime.UtcNow;

        var envelope = EventEnvelope.Create(
            new SamplePayload("hello"),
            source: "TestService",
            correlationId: Guid.NewGuid());

        Assert.NotEqual(Guid.Empty, envelope.EventId);
        Assert.InRange(envelope.OccurredOnUtc, before, DateTime.UtcNow);
    }

    [Fact]
    public void EventVersion_defaults_to_one()
    {
        var envelope = EventEnvelope.Create(
            new SamplePayload("hello"),
            source: "TestService",
            correlationId: Guid.NewGuid());

        Assert.Equal(1, envelope.EventVersion);
    }
}
