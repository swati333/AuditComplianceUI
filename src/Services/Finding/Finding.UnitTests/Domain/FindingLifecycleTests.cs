using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using Finding.Domain.Enums;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.UnitTests.Domain;

/// <summary>
/// Covers CLAUDE.md §2's finding lifecycle: Open → UnderReview →
/// ActionRequired → Resolved → Verified → Closed (strictly linear, no
/// branch), plus the explicit "High/Critical requires a corrective action"
/// gate on Resolve().
/// </summary>
public class FindingLifecycleTests
{
    private const string Actor = "reviewer@example.com";

    private static FindingEntity CreateOpenFinding(FindingSeverity severity = FindingSeverity.Low) =>
        FindingEntity.Create(Guid.NewGuid(), "Finding", null, severity, Actor);

    private static FindingEntity CreateActionRequiredFinding(FindingSeverity severity = FindingSeverity.Low)
    {
        var finding = CreateOpenFinding(severity);
        finding.StartReview(Actor);
        finding.RequireAction(Actor);
        return finding;
    }

    [Fact]
    public void StartReview_transitions_Open_to_UnderReview()
    {
        var finding = CreateOpenFinding();

        finding.StartReview(Actor);

        finding.Status.Should().Be(FindingStatus.UnderReview);
    }

    [Fact]
    public void StartReview_from_a_non_Open_status_is_rejected()
    {
        var finding = CreateActionRequiredFinding();

        var act = () => finding.StartReview(Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public void RequireAction_transitions_UnderReview_to_ActionRequired()
    {
        var finding = CreateOpenFinding();
        finding.StartReview(Actor);

        finding.RequireAction(Actor);

        finding.Status.Should().Be(FindingStatus.ActionRequired);
    }

    [Theory]
    [InlineData(FindingSeverity.Low)]
    [InlineData(FindingSeverity.Medium)]
    public void Resolve_does_not_require_a_corrective_action_for_Low_or_Medium_severity(FindingSeverity severity)
    {
        var finding = CreateActionRequiredFinding(severity);

        finding.Resolve(hasCorrectiveAction: false, Actor);

        finding.Status.Should().Be(FindingStatus.Resolved);
    }

    [Theory]
    [InlineData(FindingSeverity.High)]
    [InlineData(FindingSeverity.Critical)]
    public void Resolve_fails_without_a_corrective_action_for_High_or_Critical_severity(FindingSeverity severity)
    {
        var finding = CreateActionRequiredFinding(severity);

        var act = () => finding.Resolve(hasCorrectiveAction: false, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "CORRECTIVE_ACTION_REQUIRED");
        finding.Status.Should().Be(FindingStatus.ActionRequired);
    }

    [Theory]
    [InlineData(FindingSeverity.High)]
    [InlineData(FindingSeverity.Critical)]
    public void Resolve_succeeds_with_a_corrective_action_for_High_or_Critical_severity(FindingSeverity severity)
    {
        var finding = CreateActionRequiredFinding(severity);

        finding.Resolve(hasCorrectiveAction: true, Actor);

        finding.Status.Should().Be(FindingStatus.Resolved);
        finding.DomainEvents.Should().ContainSingle(e => e is Finding.Domain.Events.FindingResolvedDomainEvent);
    }

    [Fact]
    public void Resolve_from_a_non_ActionRequired_status_is_rejected()
    {
        var finding = CreateOpenFinding();

        var act = () => finding.Resolve(true, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public void Verify_transitions_Resolved_to_Verified()
    {
        var finding = CreateActionRequiredFinding();
        finding.Resolve(true, Actor);

        finding.Verify(Actor);

        finding.Status.Should().Be(FindingStatus.Verified);
    }

    [Fact]
    public void Close_transitions_Verified_to_Closed_and_raises_FindingClosed()
    {
        var finding = CreateActionRequiredFinding();
        finding.Resolve(true, Actor);
        finding.Verify(Actor);

        finding.Close(Actor);

        finding.Status.Should().Be(FindingStatus.Closed);
        finding.DomainEvents.Should().ContainSingle(e => e is Finding.Domain.Events.FindingClosedDomainEvent);
    }

    [Fact]
    public void Close_from_a_non_Verified_status_is_rejected()
    {
        var finding = CreateActionRequiredFinding();
        finding.Resolve(true, Actor);

        var act = () => finding.Close(Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public void There_is_no_way_to_skip_a_lifecycle_stage()
    {
        var finding = CreateOpenFinding();

        var act = () => finding.Resolve(true, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public void UpdateDetails_is_rejected_once_Closed()
    {
        var finding = CreateActionRequiredFinding();
        finding.Resolve(true, Actor);
        finding.Verify(Actor);
        finding.Close(Actor);

        var act = () => finding.UpdateDetails("New title", null, FindingSeverity.Low, Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "FINDING_NOT_EDITABLE");
    }

    [Fact]
    public void SoftDelete_sets_IsDeleted()
    {
        var finding = CreateOpenFinding();

        finding.SoftDelete(Actor);

        finding.IsDeleted.Should().BeTrue();
    }
}
