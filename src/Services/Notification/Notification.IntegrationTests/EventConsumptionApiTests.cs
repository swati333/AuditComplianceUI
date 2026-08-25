using Audit.Contracts.Events;
using Ehs.Contracts.Events;
using Finding.Contracts.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.EventHandlers;
using Notification.Contracts.ExternalEvents;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;

namespace Notification.IntegrationTests;

/// <summary>
/// Proves all 8 consumed events actually produce notifications, and that
/// consumption is idempotent (CLAUDE.md §8). No live Service Bus
/// subscription exists yet — see IntegrationEventConsumer's doc comment —
/// so this calls the consumer directly, simulating "a message arrived."
/// </summary>
public sealed class EventConsumptionApiTests : IntegrationTestBase
{
    public EventConsumptionApiTests(NotificationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task AuditPlanned_creates_a_broadcast_InApp_notification_only()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<AuditPlannedHandler>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var auditId = Guid.NewGuid();
        var envelope = EventEnvelope.Create(
            new AuditPlanned(auditId, DateTime.UtcNow, DateTime.UtcNow.AddDays(2)),
            "AuditService",
            Guid.NewGuid());

        await consumer.ConsumeAsync(envelope, "Notification.AuditPlannedHandler", handler);

        var notifications = await dbContext.Notifications.Where(n => n.SourceEventType == "AuditPlanned" && n.Body.Contains(auditId.ToString())).ToListAsync();
        notifications.Should().ContainSingle();
        notifications[0].Channel.Should().Be(NotificationChannel.InApp);
        notifications[0].RecipientUserId.Should().BeNull();
        notifications[0].Status.Should().Be(NotificationStatus.Sent);
    }

    [Fact]
    public async Task CriticalFindingCreated_creates_a_broadcast_notification()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<CriticalFindingCreatedHandler>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var findingId = Guid.NewGuid();
        var envelope = EventEnvelope.Create(
            new CriticalFindingCreated(findingId, Guid.NewGuid(), "Blocked exit", DateTime.UtcNow),
            "FindingService",
            Guid.NewGuid());

        await consumer.ConsumeAsync(envelope, "Notification.CriticalFindingCreatedHandler", handler);

        var notification = await dbContext.Notifications.SingleAsync(n => n.SourceEventType == "CriticalFindingCreated" && n.Body.Contains(findingId.ToString()));
        notification.Body.Should().Contain("Blocked exit");
    }

    [Fact]
    public async Task ActionPlanAssigned_creates_notifications_addressed_to_the_assignee_on_both_channels()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<ActionPlanAssignedHandler>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var actionPlanId = Guid.NewGuid();
        var assignee = $"assignee-{Guid.NewGuid()}";
        var envelope = EventEnvelope.Create(
            new ActionPlanAssigned(actionPlanId, Guid.NewGuid(), assignee, "Fix the exit signage", DateTime.UtcNow.AddDays(7), DateTime.UtcNow),
            "ActionPlanService",
            Guid.NewGuid());

        await consumer.ConsumeAsync(envelope, "Notification.ActionPlanAssignedHandler", handler);

        var notifications = await dbContext.Notifications.Where(n => n.RecipientUserId == assignee).ToListAsync();
        notifications.Select(n => n.Channel).Should().BeEquivalentTo([NotificationChannel.Email, NotificationChannel.InApp]);
    }

    [Fact]
    public async Task ActionPlanRejected_addresses_the_assignee_not_the_approver()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<ActionPlanRejectedHandler>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var assignee = $"assignee-{Guid.NewGuid()}";
        var envelope = EventEnvelope.Create(
            new ActionPlanRejected(Guid.NewGuid(), Guid.NewGuid(), assignee, "approver-1", "Insufficient evidence.", DateTime.UtcNow),
            "ActionPlanService",
            Guid.NewGuid());

        await consumer.ConsumeAsync(envelope, "Notification.ActionPlanRejectedHandler", handler);

        var notifications = await dbContext.Notifications.Where(n => n.SourceEventType == "ActionPlanRejected" && n.CorrelationId == envelope.CorrelationId).ToListAsync();
        notifications.Should().OnlyContain(n => n.RecipientUserId == assignee);
        notifications.Should().Contain(n => n.Body.Contains("Insufficient evidence."));
    }

    [Fact]
    public async Task ActionPlanOverdue_and_ActionPlanSubmitted_and_ReportGenerated_and_ReportGenerationFailed_all_create_notifications()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var correlationId = Guid.NewGuid();

        var overdueUser = $"user-{Guid.NewGuid()}";
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ActionPlanOverdue(Guid.NewGuid(), Guid.NewGuid(), overdueUser, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow), "ActionPlanService", correlationId),
            "Notification.ActionPlanOverdueHandler",
            scope.ServiceProvider.GetRequiredService<ActionPlanOverdueHandler>());

        var submitter = $"user-{Guid.NewGuid()}";
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ActionPlanSubmitted(Guid.NewGuid(), Guid.NewGuid(), submitter, DateTime.UtcNow), "ActionPlanService", correlationId),
            "Notification.ActionPlanSubmittedHandler",
            scope.ServiceProvider.GetRequiredService<ActionPlanSubmittedHandler>());

        var reportRequester = $"user-{Guid.NewGuid()}";
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ReportGenerated(Guid.NewGuid(), "ComplianceSummary", reportRequester, "blob://reports/x.pdf", DateTime.UtcNow), "ReportingService", correlationId),
            "Notification.ReportGeneratedHandler",
            scope.ServiceProvider.GetRequiredService<ReportGeneratedHandler>());

        var failedRequester = $"user-{Guid.NewGuid()}";
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ReportGenerationFailed(Guid.NewGuid(), "ComplianceSummary", failedRequester, "Timeout", DateTime.UtcNow), "ReportingService", correlationId),
            "Notification.ReportGenerationFailedHandler",
            scope.ServiceProvider.GetRequiredService<ReportGenerationFailedHandler>());

        (await dbContext.Notifications.AnyAsync(n => n.RecipientUserId == overdueUser)).Should().BeTrue();
        (await dbContext.Notifications.AnyAsync(n => n.RecipientUserId == submitter)).Should().BeTrue();
        (await dbContext.Notifications.AnyAsync(n => n.RecipientUserId == reportRequester)).Should().BeTrue();
        (await dbContext.Notifications.AnyAsync(n => n.RecipientUserId == failedRequester && n.Body.Contains("Timeout"))).Should().BeTrue();
    }

    [Fact]
    public async Task Redelivering_the_same_event_does_not_duplicate_notifications()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<AuditPlannedHandler>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var auditId = Guid.NewGuid();
        var envelope = EventEnvelope.Create(new AuditPlanned(auditId, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)), "AuditService", Guid.NewGuid());

        var first = await consumer.ConsumeAsync(envelope, "Notification.AuditPlannedHandler", handler);
        var second = await consumer.ConsumeAsync(envelope, "Notification.AuditPlannedHandler", handler);

        first.Should().BeTrue();
        second.Should().BeFalse("a redelivery of the same eventId must be a no-op");

        var count = await dbContext.Notifications.CountAsync(n => n.Body.Contains(auditId.ToString()));
        count.Should().Be(1);
    }
}
