namespace Audit.Application.Common;

/// <summary>
/// Port onto the "does this audit have open Critical findings / mandatory
/// actions" question that CLAUDE.md §2 requires Close() to check. Finding
/// Service and Action Plan Service own that data (CLAUDE.md §5: a service
/// never directly reads another service's tables), so the Infrastructure
/// implementation answers from a local read model kept in sync via their
/// integration events (CriticalFindingCreated/FindingResolved,
/// ActionPlanAssigned/ActionPlanStatusChanged) — see Audit.Infrastructure's
/// AuditComplianceGateway and Audit.Application.EventHandlers.
/// </summary>
public interface IAuditComplianceGateway
{
    Task<AuditComplianceStatus> GetComplianceStatusAsync(Guid auditId, CancellationToken cancellationToken = default);
}

public sealed record AuditComplianceStatus(bool HasOpenCriticalFindings, bool HasOpenRequiredActions);
