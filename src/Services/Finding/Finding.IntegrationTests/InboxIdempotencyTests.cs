using System.Net.Http.Json;
using Audit.Contracts.Events;
using Ehs.Contracts.Events;
using Finding.Application.EventHandlers;
using Finding.Infrastructure.Messaging;
using Finding.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finding.IntegrationTests;

/// <summary>
/// Proves the Inbox pattern actually works (CLAUDE.md §8: "duplicate
/// delivery must never duplicate a business operation") against a real
/// database. There is no live Service Bus subscription to receive these
/// messages from in this phase (see IntegrationEventConsumer's doc comment)
/// — this simulates "a message arrived, and then arrived again" by calling
/// the consumer directly, exactly as a future Service-Bus-triggered
/// background worker would.
/// </summary>
public sealed class InboxIdempotencyTests : IntegrationTestBase
{
    public InboxIdempotencyTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Consuming_AuditCreated_twice_creates_only_one_AuditReference()
    {
        var auditId = Guid.NewGuid();
        var envelope = EventEnvelope.Create(
            new AuditCreated(auditId, "Redelivery Test Audit", "Scope", "Location", DateTime.UtcNow),
            source: "AuditService",
            correlationId: Guid.NewGuid());

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<AuditCreatedHandler>();

        var firstResult = await consumer.ConsumeAsync(envelope, "Finding.AuditCreatedHandler", handler);
        var secondResult = await consumer.ConsumeAsync(envelope, "Finding.AuditCreatedHandler", handler);

        firstResult.Should().BeTrue("the first delivery should run the handler");
        secondResult.Should().BeFalse("a redelivery of the same eventId must be a no-op");

        var dbContext = scope.ServiceProvider.GetRequiredService<FindingDbContext>();
        var referenceCount = await dbContext.AuditReferences.CountAsync(r => r.AuditId == auditId);
        referenceCount.Should().Be(1);
    }

    [Fact]
    public async Task Consuming_AuditClosed_marks_the_local_reference_closed()
    {
        var auditId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();

        var createdHandler = scope.ServiceProvider.GetRequiredService<AuditCreatedHandler>();
        var createdEnvelope = EventEnvelope.Create(
            new AuditCreated(auditId, "Audit To Close", "Scope", "Location", DateTime.UtcNow),
            source: "AuditService",
            correlationId: Guid.NewGuid());
        await consumer.ConsumeAsync(createdEnvelope, "Finding.AuditCreatedHandler", createdHandler);

        var closedHandler = scope.ServiceProvider.GetRequiredService<AuditClosedHandler>();
        var closedEnvelope = EventEnvelope.Create(
            new AuditClosed(auditId, DateTime.UtcNow),
            source: "AuditService",
            correlationId: Guid.NewGuid());
        await consumer.ConsumeAsync(closedEnvelope, "Finding.AuditClosedHandler", closedHandler);

        var dbContext = scope.ServiceProvider.GetRequiredService<FindingDbContext>();
        var reference = await dbContext.AuditReferences.SingleAsync(r => r.AuditId == auditId);
        reference.IsClosed.Should().BeTrue();
    }

    [Fact]
    public async Task A_finding_cannot_be_created_against_an_audit_the_local_reference_shows_as_closed()
    {
        var auditId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var createdHandler = scope.ServiceProvider.GetRequiredService<AuditCreatedHandler>();
        var closedHandler = scope.ServiceProvider.GetRequiredService<AuditClosedHandler>();

        await consumer.ConsumeAsync(
            EventEnvelope.Create(new AuditCreated(auditId, "Closed Audit", "Scope", "Location", DateTime.UtcNow), "AuditService", Guid.NewGuid()),
            "Finding.AuditCreatedHandler",
            createdHandler);
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new AuditClosed(auditId, DateTime.UtcNow), "AuditService", Guid.NewGuid()),
            "Finding.AuditClosedHandler",
            closedHandler);

        var response = await Client.PostAsJsonAsync(
            "/api/v1/findings",
            new Finding.Contracts.Requests.CreateFindingRequest(auditId, "Late finding", null, "Low"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Conflict);
    }
}
