namespace Audit.Application.Common;

/// <summary>
/// Port onto the "does this audit have open Critical findings / required
/// actions" question that CLAUDE.md §2 requires Close() to check. Finding
/// Service and Action Plan Service own that data (CLAUDE.md §5: a service
/// never directly reads another service's tables) and would normally answer
/// this from a local read-model kept in sync via their integration events
/// (CriticalFindingCreated/Resolved, ActionPlanAssigned/Closed). Neither
/// service exists yet in this "Audit Service only" phase, so the
/// Infrastructure implementation is a documented placeholder — see
/// Audit.Infrastructure's AuditComplianceGateway.
/// </summary>
public interface IAuditComplianceGateway
{
    Task<AuditComplianceStatus> GetComplianceStatusAsync(Guid auditId, CancellationToken cancellationToken = default);
}

public sealed record AuditComplianceStatus(bool HasOpenCriticalFindings, bool HasOpenRequiredActions);
