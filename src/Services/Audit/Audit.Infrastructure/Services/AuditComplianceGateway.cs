using Audit.Application.Common;

namespace Audit.Infrastructure.Services;

/// <summary>
/// Answers Audit.Close()'s "are there open Critical findings / mandatory
/// actions" question from two local read models —
/// <see cref="Audit.Domain.Entities.OpenCriticalFinding"/> and
/// <see cref="Audit.Domain.Entities.OpenRequiredAction"/> — kept in sync by
/// inbox-consuming Finding Service's CriticalFindingCreated/FindingResolved
/// and Action Plan Service's ActionPlanAssigned/ActionPlanStatusChanged
/// integration events (see Audit.Application.EventHandlers). Never queries
/// FindingDb or ActionPlanDb directly (CLAUDE.md §5).
/// </summary>
public sealed class AuditComplianceGateway : IAuditComplianceGateway
{
    private readonly IOpenCriticalFindingRepository _openCriticalFindingRepository;
    private readonly IOpenRequiredActionRepository _openRequiredActionRepository;

    public AuditComplianceGateway(
        IOpenCriticalFindingRepository openCriticalFindingRepository,
        IOpenRequiredActionRepository openRequiredActionRepository)
    {
        _openCriticalFindingRepository = openCriticalFindingRepository;
        _openRequiredActionRepository = openRequiredActionRepository;
    }

    public async Task<AuditComplianceStatus> GetComplianceStatusAsync(Guid auditId, CancellationToken cancellationToken = default)
    {
        var hasOpenCriticalFindings = await _openCriticalFindingRepository.ExistsForAuditAsync(auditId, cancellationToken);
        var hasOpenRequiredActions = await _openRequiredActionRepository.ExistsForAuditAsync(auditId, cancellationToken);
        return new AuditComplianceStatus(hasOpenCriticalFindings, hasOpenRequiredActions);
    }
}
