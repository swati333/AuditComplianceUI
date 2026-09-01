namespace Finding.Domain.Entities;

/// <summary>
/// Local, eventually-consistent projection of "this finding has at least one
/// corrective action" — populated exclusively by consuming Action Plan
/// Service's ActionPlanAssigned integration event, never by querying
/// ActionPlanDb (CLAUDE.md §5). Backs <c>IActionPlanGateway</c>, which
/// Finding.Domain.Entities.Finding.Resolve() uses to enforce "High/Critical
/// findings require at least one corrective action" (CLAUDE.md §2). One row
/// per FindingId that has ever had an action plan assigned to it — presence
/// of the row is the answer; it is never removed even if the action plan is
/// later closed or cancelled, since the requirement is that one was created,
/// not that one remains open.
/// </summary>
public sealed class ActionPlanReference
{
    public Guid FindingId { get; private set; }

    public Guid FirstActionPlanId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    private ActionPlanReference()
    {
    }

    public static ActionPlanReference Create(Guid findingId, Guid actionPlanId) =>
        new()
        {
            FindingId = findingId,
            FirstActionPlanId = actionPlanId,
            CreatedAtUtc = DateTime.UtcNow,
        };
}
