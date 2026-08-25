using Finding.Application.Common;
using Microsoft.Extensions.Logging;

namespace Finding.Infrastructure.Services;

/// <summary>
/// Placeholder implementation of <see cref="IActionPlanGateway"/>. Action
/// Plan Service (CLAUDE.md phase 4) doesn't exist yet, so there is no
/// corrective-action data anywhere to check — this always reports "yes, a
/// corrective action exists," the permissive default that keeps the
/// Resolve() happy path reachable in this phase rather than permanently
/// blocking every High/Critical finding. The Resolve() business rule itself
/// is fully implemented and unit-tested (Finding.Domain.Entities.Finding.Resolve)
/// with both true and false regardless of what this placeholder returns.
///
/// The intended real implementation: a local read model in FindingDb kept in
/// sync by inbox-consuming Action Plan Service's ActionPlanAssigned event
/// (CLAUDE.md §5 — no cross-service database reads). Swapping this class for
/// that one is the only change Resolve() would ever need.
/// </summary>
public sealed class ActionPlanGateway : IActionPlanGateway
{
    private readonly ILogger<ActionPlanGateway> _logger;

    public ActionPlanGateway(ILogger<ActionPlanGateway> logger)
    {
        _logger = logger;
    }

    public Task<bool> HasCorrectiveActionAsync(Guid findingId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "ActionPlanGateway placeholder queried for finding {FindingId}: Action Plan Service is not implemented yet, reporting a corrective action exists.",
            findingId);
        return Task.FromResult(true);
    }
}
