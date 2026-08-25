namespace Finding.Application.Common;

/// <summary>
/// Port onto "does this finding have at least one corrective action" —
/// CLAUDE.md §2's rule that High/Critical findings require one before they
/// can resolve. Action Plan Service owns that data (CLAUDE.md §5) and
/// doesn't exist yet in this "Finding Service only" phase, so the
/// Infrastructure implementation is a documented placeholder — see
/// Finding.Infrastructure's ActionPlanGateway.
/// </summary>
public interface IActionPlanGateway
{
    Task<bool> HasCorrectiveActionAsync(Guid findingId, CancellationToken cancellationToken = default);
}
