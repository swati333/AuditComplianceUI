namespace Finding.Application.Common;

/// <summary>
/// Port onto "does this finding have at least one corrective action" —
/// CLAUDE.md §2's rule that High/Critical findings require one before they
/// can resolve. Action Plan Service owns that data (CLAUDE.md §5), so the
/// Infrastructure implementation answers from a local read model kept in
/// sync via Action Plan Service's ActionPlanAssigned integration event — see
/// Finding.Infrastructure's ActionPlanGateway.
/// </summary>
public interface IActionPlanGateway
{
    Task<bool> HasCorrectiveActionAsync(Guid findingId, CancellationToken cancellationToken = default);
}
