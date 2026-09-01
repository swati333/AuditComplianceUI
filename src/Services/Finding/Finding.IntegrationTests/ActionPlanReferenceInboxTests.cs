using ActionPlan.Contracts.Events;
using Ehs.Contracts.Events;
using Finding.Application.Common;
using Finding.Application.EventHandlers;
using Finding.Infrastructure.Messaging;
using Finding.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finding.IntegrationTests;

/// <summary>
/// Proves IActionPlanGateway is now backed by a real, inbox-synced read
/// model rather than the old always-true placeholder (CLAUDE.md §8 —
/// duplicate delivery must never duplicate a business effect).
/// </summary>
public sealed class ActionPlanReferenceInboxTests : IntegrationTestBase
{
    public ActionPlanReferenceInboxTests(FindingApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Consuming_ActionPlanAssigned_twice_creates_only_one_ActionPlanReference()
    {
        var findingId = Guid.NewGuid();
        var envelope = EventEnvelope.Create(
            new ActionPlanAssigned(Guid.NewGuid(), findingId, "owner-1", "approver-1", DateTime.UtcNow.AddDays(7), "High", DateTime.UtcNow),
            source: "ActionPlanService",
            correlationId: Guid.NewGuid());

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<ActionPlanAssignedHandler>();

        var firstResult = await consumer.ConsumeAsync(envelope, "Finding.ActionPlanAssignedHandler", handler);
        var secondResult = await consumer.ConsumeAsync(envelope, "Finding.ActionPlanAssignedHandler", handler);

        firstResult.Should().BeTrue("the first delivery should run the handler");
        secondResult.Should().BeFalse("a redelivery of the same eventId must be a no-op");

        var dbContext = scope.ServiceProvider.GetRequiredService<FindingDbContext>();
        var referenceCount = await dbContext.ActionPlanReferences.CountAsync(r => r.FindingId == findingId);
        referenceCount.Should().Be(1);
    }

    [Fact]
    public async Task Gateway_reports_no_corrective_action_until_ActionPlanAssigned_is_consumed()
    {
        var findingId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IActionPlanGateway>();

        (await gateway.HasCorrectiveActionAsync(findingId)).Should().BeFalse("no action plan has been assigned to this finding yet");

        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<ActionPlanAssignedHandler>();
        var envelope = EventEnvelope.Create(
            new ActionPlanAssigned(Guid.NewGuid(), findingId, "owner-1", "approver-1", DateTime.UtcNow.AddDays(7), "High", DateTime.UtcNow),
            source: "ActionPlanService",
            correlationId: Guid.NewGuid());
        await consumer.ConsumeAsync(envelope, "Finding.ActionPlanAssignedHandler", handler);

        (await gateway.HasCorrectiveActionAsync(findingId)).Should().BeTrue("ActionPlanAssigned has now been consumed for this finding");
    }
}
