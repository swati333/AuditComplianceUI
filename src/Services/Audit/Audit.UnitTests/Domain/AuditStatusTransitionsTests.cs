using Audit.Domain;
using Audit.Domain.Enums;
using FluentAssertions;

namespace Audit.UnitTests.Domain;

/// <summary>Exhaustively verifies the state machine graph from CLAUDE.md §2.</summary>
public class AuditStatusTransitionsTests
{
    public static IEnumerable<object[]> AllowedTransitions =>
    [
        [AuditStatus.Draft, AuditStatus.Planned],
        [AuditStatus.Draft, AuditStatus.Cancelled],
        [AuditStatus.Planned, AuditStatus.InProgress],
        [AuditStatus.Planned, AuditStatus.Cancelled],
        [AuditStatus.InProgress, AuditStatus.Completed],
        [AuditStatus.InProgress, AuditStatus.Cancelled],
        [AuditStatus.Completed, AuditStatus.Closed],
    ];

    public static IEnumerable<object[]> DisallowedTransitions =>
    [
        [AuditStatus.Draft, AuditStatus.InProgress],
        [AuditStatus.Draft, AuditStatus.Completed],
        [AuditStatus.Draft, AuditStatus.Closed],
        [AuditStatus.Planned, AuditStatus.Completed],
        [AuditStatus.Planned, AuditStatus.Closed],
        [AuditStatus.InProgress, AuditStatus.Closed],
        [AuditStatus.InProgress, AuditStatus.Planned],
        [AuditStatus.Completed, AuditStatus.Cancelled],
        [AuditStatus.Completed, AuditStatus.InProgress],
        [AuditStatus.Closed, AuditStatus.Cancelled],
        [AuditStatus.Cancelled, AuditStatus.Draft],
        [AuditStatus.Cancelled, AuditStatus.Planned],
    ];

    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public void Allows_every_transition_in_the_lifecycle_diagram(AuditStatus from, AuditStatus to)
    {
        AuditStatusTransitions.IsAllowed(from, to).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(DisallowedTransitions))]
    public void Rejects_every_transition_outside_the_lifecycle_diagram(AuditStatus from, AuditStatus to)
    {
        AuditStatusTransitions.IsAllowed(from, to).Should().BeFalse();
    }

    [Theory]
    [InlineData(AuditStatus.Closed)]
    [InlineData(AuditStatus.Cancelled)]
    public void Terminal_states_allow_no_further_transitions(AuditStatus terminal)
    {
        foreach (AuditStatus target in Enum.GetValues<AuditStatus>())
        {
            AuditStatusTransitions.IsAllowed(terminal, target).Should().BeFalse();
        }
    }
}
