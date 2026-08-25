using Finding.Domain;
using Finding.Domain.Enums;
using FluentAssertions;

namespace Finding.UnitTests.Domain;

public class FindingStatusTransitionsTests
{
    public static IEnumerable<object[]> AllowedTransitions =>
    [
        [FindingStatus.Open, FindingStatus.UnderReview],
        [FindingStatus.UnderReview, FindingStatus.ActionRequired],
        [FindingStatus.ActionRequired, FindingStatus.Resolved],
        [FindingStatus.Resolved, FindingStatus.Verified],
        [FindingStatus.Verified, FindingStatus.Closed],
    ];

    public static IEnumerable<object[]> DisallowedTransitions =>
    [
        [FindingStatus.Open, FindingStatus.ActionRequired],
        [FindingStatus.Open, FindingStatus.Resolved],
        [FindingStatus.Open, FindingStatus.Verified],
        [FindingStatus.Open, FindingStatus.Closed],
        [FindingStatus.UnderReview, FindingStatus.Resolved],
        [FindingStatus.UnderReview, FindingStatus.Open],
        [FindingStatus.ActionRequired, FindingStatus.Verified],
        [FindingStatus.ActionRequired, FindingStatus.UnderReview],
        [FindingStatus.Resolved, FindingStatus.Closed],
        [FindingStatus.Resolved, FindingStatus.ActionRequired],
        [FindingStatus.Verified, FindingStatus.Resolved],
        [FindingStatus.Closed, FindingStatus.Open],
    ];

    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public void Allows_every_transition_in_the_lifecycle_diagram(FindingStatus from, FindingStatus to)
    {
        FindingStatusTransitions.IsAllowed(from, to).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(DisallowedTransitions))]
    public void Rejects_every_transition_outside_the_lifecycle_diagram(FindingStatus from, FindingStatus to)
    {
        FindingStatusTransitions.IsAllowed(from, to).Should().BeFalse();
    }

    [Fact]
    public void Closed_allows_no_further_transitions()
    {
        foreach (FindingStatus target in Enum.GetValues<FindingStatus>())
        {
            FindingStatusTransitions.IsAllowed(FindingStatus.Closed, target).Should().BeFalse();
        }
    }
}
