using Audit.Application.Common;
using Microsoft.Extensions.Logging;

namespace Audit.Infrastructure.Services;

/// <summary>
/// Placeholder implementation of <see cref="IAuditComplianceGateway"/>.
/// Finding Service and Action Plan Service (CLAUDE.md phases 3-4) don't
/// exist yet, so there is no Critical-finding/required-action data anywhere
/// to check — this always reports "nothing open," which is the only
/// currently-correct answer, not a shortcut around the rule. The
/// Close() business rule itself is fully implemented and unit-tested
/// (Audit.Domain.Entities.Audit.Close) regardless of what this returns.
///
/// The intended real implementation: a local read model in AuditDb kept in
/// sync by inbox-consuming FindingService's CriticalFindingCreated/Resolved
/// and ActionPlanService's ActionPlanAssigned/Closed integration events
/// (CLAUDE.md §5 — no cross-service database reads). Swapping this class for
/// that one is the only change Close() would ever need.
/// </summary>
public sealed class AuditComplianceGateway : IAuditComplianceGateway
{
    private readonly ILogger<AuditComplianceGateway> _logger;

    public AuditComplianceGateway(ILogger<AuditComplianceGateway> logger)
    {
        _logger = logger;
    }

    public Task<AuditComplianceStatus> GetComplianceStatusAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "AuditComplianceGateway placeholder queried for audit {AuditId}: Finding/Action Plan services are not implemented yet, reporting no open items.",
            auditId);
        return Task.FromResult(new AuditComplianceStatus(HasOpenCriticalFindings: false, HasOpenRequiredActions: false));
    }
}
