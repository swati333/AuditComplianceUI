using Audit.Domain.Enums;
using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.UnitTests.Domain;

/// <summary>
/// Covers CLAUDE.md §2's lifecycle rules directly on the aggregate:
/// Draft → Planned → InProgress → Completed → Closed, cancellation, and the
/// three explicit guard rules (team/scope/location before Planned, mandatory
/// questions before Completed, no open Critical findings/actions before Closed).
/// </summary>
public class AuditLifecycleTests
{
    private const string Actor = "auditor@example.com";

    private static AuditEntity CreateDraftAudit() =>
        AuditEntity.Create("Annual EHS Audit", "desc", "Full site", "Plant A", Actor);

    private static AuditEntity CreatePlannableAudit()
    {
        var audit = CreateDraftAudit();
        audit.AssignTeamMember("user-1", "Alice Auditor", AuditTeamRole.Auditor, Actor);
        return audit;
    }

    private static AuditEntity CreatePlannedAudit()
    {
        var audit = CreatePlannableAudit();
        audit.Plan(DateTime.UtcNow.Date.AddDays(1), DateTime.UtcNow.Date.AddDays(3), Actor);
        return audit;
    }

    private static AuditEntity CreateInProgressAudit()
    {
        var audit = CreatePlannedAudit();
        audit.Start(Actor);
        return audit;
    }

    // --- Plan() ---

    [Fact]
    public void Plan_fails_when_no_team_is_assigned()
    {
        var audit = CreateDraftAudit();

        var act = () => audit.Plan(DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "AUDIT_TEAM_NOT_ASSIGNED");
    }

    [Fact]
    public void Plan_fails_when_end_date_is_before_start_date()
    {
        var audit = CreatePlannableAudit();

        var act = () => audit.Plan(DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(1), Actor);

        act.Should().Throw<BusinessValidationException>();
    }

    [Fact]
    public void Plan_succeeds_and_transitions_to_Planned()
    {
        var audit = CreatePlannableAudit();
        var start = DateTime.UtcNow.Date.AddDays(1);
        var end = DateTime.UtcNow.Date.AddDays(3);

        audit.Plan(start, end, Actor);

        audit.Status.Should().Be(AuditStatus.Planned);
        audit.PlannedStartDate.Should().Be(start);
        audit.PlannedEndDate.Should().Be(end);
        audit.DomainEvents.Should().ContainSingle(e => e is Audit.Domain.Events.AuditPlannedDomainEvent);
    }

    [Fact]
    public void Plan_from_a_non_Draft_status_is_rejected()
    {
        var audit = CreatePlannedAudit();

        var act = () => audit.Plan(DateTime.UtcNow, DateTime.UtcNow.AddDays(1), Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    // --- Start() ---

    [Fact]
    public void Start_only_succeeds_from_Planned()
    {
        var audit = CreatePlannedAudit();

        audit.Start(Actor);

        audit.Status.Should().Be(AuditStatus.InProgress);
        audit.ActualStartDate.Should().NotBeNull();
        audit.DomainEvents.Should().ContainSingle(e => e is Audit.Domain.Events.AuditStartedDomainEvent);
    }

    [Fact]
    public void Start_from_Draft_is_rejected()
    {
        var audit = CreateDraftAudit();

        var act = () => audit.Start(Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    // --- Complete() ---

    [Fact]
    public void Complete_fails_when_mandatory_questions_are_unanswered()
    {
        var audit = CreateInProgressAudit();
        var mandatoryQuestionId = Guid.NewGuid();

        var act = () => audit.Complete([mandatoryQuestionId], Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "MANDATORY_QUESTIONS_UNANSWERED");
    }

    [Fact]
    public void Complete_succeeds_once_all_mandatory_questions_are_answered()
    {
        var audit = CreateInProgressAudit();
        var mandatoryQuestionId = Guid.NewGuid();
        audit.RecordChecklistResponse(mandatoryQuestionId, "Yes, compliant.", isCompliant: true, Actor);

        audit.Complete([mandatoryQuestionId], Actor);

        audit.Status.Should().Be(AuditStatus.Completed);
        audit.ActualEndDate.Should().NotBeNull();
        audit.DomainEvents.Should().ContainSingle(e => e is Audit.Domain.Events.AuditCompletedDomainEvent);
    }

    [Fact]
    public void Complete_succeeds_immediately_when_there_are_no_mandatory_questions()
    {
        var audit = CreateInProgressAudit();

        audit.Complete(mandatoryQuestionIds: [], Actor);

        audit.Status.Should().Be(AuditStatus.Completed);
    }

    // --- Close() ---

    [Fact]
    public void Close_fails_while_Critical_findings_remain_open()
    {
        var audit = CreateInProgressAudit();
        audit.Complete([], Actor);

        var act = () => audit.Close(hasOpenCriticalFindings: true, hasOpenRequiredActions: false, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "OPEN_CRITICAL_FINDINGS");
    }

    [Fact]
    public void Close_fails_while_required_actions_remain_open()
    {
        var audit = CreateInProgressAudit();
        audit.Complete([], Actor);

        var act = () => audit.Close(hasOpenCriticalFindings: false, hasOpenRequiredActions: true, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "OPEN_REQUIRED_ACTIONS");
    }

    [Fact]
    public void Close_succeeds_when_nothing_is_open()
    {
        var audit = CreateInProgressAudit();
        audit.Complete([], Actor);

        audit.Close(hasOpenCriticalFindings: false, hasOpenRequiredActions: false, Actor);

        audit.Status.Should().Be(AuditStatus.Closed);
        audit.DomainEvents.Should().ContainSingle(e => e is Audit.Domain.Events.AuditClosedDomainEvent);
    }

    [Fact]
    public void Close_from_InProgress_without_completing_first_is_rejected()
    {
        var audit = CreateInProgressAudit();

        var act = () => audit.Close(false, false, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    // --- Cancel() ---

    [Theory]
    [InlineData(AuditStatus.Draft)]
    [InlineData(AuditStatus.Planned)]
    [InlineData(AuditStatus.InProgress)]
    public void Cancel_is_permitted_from_early_states(AuditStatus fromStatus)
    {
        var audit = fromStatus switch
        {
            AuditStatus.Draft => CreateDraftAudit(),
            AuditStatus.Planned => CreatePlannedAudit(),
            AuditStatus.InProgress => CreateInProgressAudit(),
            _ => throw new ArgumentOutOfRangeException(nameof(fromStatus)),
        };

        audit.Cancel("No longer required", Actor);

        audit.Status.Should().Be(AuditStatus.Cancelled);
        audit.CancellationReason.Should().Be("No longer required");
    }

    [Fact]
    public void Cancel_is_rejected_once_Completed()
    {
        var audit = CreateInProgressAudit();
        audit.Complete([], Actor);

        var act = () => audit.Cancel("too late", Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    // --- Editability guard ---

    [Fact]
    public void UpdateDetails_is_rejected_once_Closed()
    {
        var audit = CreateInProgressAudit();
        audit.Complete([], Actor);
        audit.Close(false, false, Actor);

        var act = () => audit.UpdateDetails("New title", null, "Scope", "Location", Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "AUDIT_NOT_EDITABLE");
    }

    [Fact]
    public void SoftDelete_sets_IsDeleted()
    {
        var audit = CreateDraftAudit();

        audit.SoftDelete(Actor);

        audit.IsDeleted.Should().BeTrue();
    }
}
