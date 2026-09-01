using ActionPlan.Contracts.Events;
using Audit.Application.Common;
using Audit.Application.EventHandlers;
using Audit.Infrastructure.Messaging;
using Ehs.Contracts.Events;
using Finding.Contracts.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.IntegrationTests;

/// <summary>
/// Proves IAuditComplianceGateway is now backed by a real, inbox-synced read
/// model rather than the old always-"nothing open" placeholder (CLAUDE.md
/// §8 — duplicate delivery must never duplicate a business effect). This is
/// Audit Service's first consumer of another service's events.
/// </summary>
public sealed class ComplianceGatewayInboxTests : IntegrationTestBase
{
    public ComplianceGatewayInboxTests(AuditApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Gateway_reports_open_critical_finding_after_CriticalFindingCreated_and_clears_it_after_FindingResolved()
    {
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        var gateway = scope.ServiceProvider.GetRequiredService<IAuditComplianceGateway>();

        (await gateway.GetComplianceStatusAsync(auditId)).HasOpenCriticalFindings.Should().BeFalse();

        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var createdHandler = scope.ServiceProvider.GetRequiredService<CriticalFindingCreatedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new CriticalFindingCreated(findingId, auditId, "Uncontrolled hot work", DateTime.UtcNow), "FindingService", Guid.NewGuid()),
            "Audit.CriticalFindingCreatedHandler",
            createdHandler);

        (await gateway.GetComplianceStatusAsync(auditId)).HasOpenCriticalFindings.Should().BeTrue("a Critical finding was just raised against this audit");

        var resolvedHandler = scope.ServiceProvider.GetRequiredService<FindingResolvedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new FindingResolved(findingId, auditId, DateTime.UtcNow), "FindingService", Guid.NewGuid()),
            "Audit.FindingResolvedHandler",
            resolvedHandler);

        (await gateway.GetComplianceStatusAsync(auditId)).HasOpenCriticalFindings.Should().BeFalse("the Critical finding has since been resolved");
    }

    [Fact]
    public async Task Gateway_reports_open_required_action_for_a_High_finding_and_clears_it_when_the_action_closes()
    {
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();
        var actionPlanId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var gateway = scope.ServiceProvider.GetRequiredService<IAuditComplianceGateway>();

        var findingCreatedHandler = scope.ServiceProvider.GetRequiredService<FindingCreatedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new FindingCreated(findingId, auditId, "Missing guard rail", "High", DateTime.UtcNow), "FindingService", Guid.NewGuid()),
            "Audit.FindingCreatedHandler",
            findingCreatedHandler);

        var assignedHandler = scope.ServiceProvider.GetRequiredService<ActionPlanAssignedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ActionPlanAssigned(actionPlanId, findingId, "owner-1", "approver-1", DateTime.UtcNow.AddDays(7), "High", DateTime.UtcNow), "ActionPlanService", Guid.NewGuid()),
            "Audit.ActionPlanAssignedHandler",
            assignedHandler);

        (await gateway.GetComplianceStatusAsync(auditId)).HasOpenRequiredActions.Should().BeTrue("a mandatory action was just assigned for this audit's High finding");

        var statusChangedHandler = scope.ServiceProvider.GetRequiredService<ActionPlanStatusChangedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ActionPlanStatusChanged(actionPlanId, findingId, "Approved", "Closed", DateTime.UtcNow), "ActionPlanService", Guid.NewGuid()),
            "Audit.ActionPlanStatusChangedHandler",
            statusChangedHandler);

        (await gateway.GetComplianceStatusAsync(auditId)).HasOpenRequiredActions.Should().BeFalse("the action plan has since been closed");
    }

    [Fact]
    public async Task Gateway_does_not_track_an_action_plan_assigned_to_a_Low_severity_finding()
    {
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();
        var actionPlanId = Guid.NewGuid();

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var gateway = scope.ServiceProvider.GetRequiredService<IAuditComplianceGateway>();

        var findingCreatedHandler = scope.ServiceProvider.GetRequiredService<FindingCreatedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new FindingCreated(findingId, auditId, "Minor housekeeping issue", "Low", DateTime.UtcNow), "FindingService", Guid.NewGuid()),
            "Audit.FindingCreatedHandler",
            findingCreatedHandler);

        var assignedHandler = scope.ServiceProvider.GetRequiredService<ActionPlanAssignedHandler>();
        await consumer.ConsumeAsync(
            EventEnvelope.Create(new ActionPlanAssigned(actionPlanId, findingId, "owner-1", "approver-1", DateTime.UtcNow.AddDays(7), "Low", DateTime.UtcNow), "ActionPlanService", Guid.NewGuid()),
            "Audit.ActionPlanAssignedHandler",
            assignedHandler);

        (await gateway.GetComplianceStatusAsync(auditId)).HasOpenRequiredActions.Should().BeFalse("an action on a Low-severity finding is optional, not mandatory");
    }

    [Fact]
    public async Task Consuming_CriticalFindingCreated_twice_opens_only_one_entry()
    {
        var auditId = Guid.NewGuid();
        var findingId = Guid.NewGuid();
        var envelope = EventEnvelope.Create(new CriticalFindingCreated(findingId, auditId, "Redelivery test", DateTime.UtcNow), "FindingService", Guid.NewGuid());

        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<CriticalFindingCreatedHandler>();

        var firstResult = await consumer.ConsumeAsync(envelope, "Audit.CriticalFindingCreatedHandler", handler);
        var secondResult = await consumer.ConsumeAsync(envelope, "Audit.CriticalFindingCreatedHandler", handler);

        firstResult.Should().BeTrue("the first delivery should run the handler");
        secondResult.Should().BeFalse("a redelivery of the same eventId must be a no-op");
    }
}
