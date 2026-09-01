namespace Audit.Domain.Entities;

/// <summary>
/// Local projection of "this audit has a Critical finding that is not yet
/// resolved" — populated exclusively by consuming Finding Service's
/// CriticalFindingCreated integration event (row added) and FindingResolved
/// (row removed), never by querying FindingDb (CLAUDE.md §5). Backs
/// <c>IAuditComplianceGateway.GetComplianceStatusAsync</c>'s
/// HasOpenCriticalFindings answer, which
/// Audit.Domain.Entities.Audit.Close() uses to enforce "an audit cannot
/// close while ... Critical findings are open" (CLAUDE.md §2). Presence of a
/// row for a given AuditId is the answer — there is nothing else to store.
/// </summary>
public sealed class OpenCriticalFinding
{
    public Guid FindingId { get; private set; }

    public Guid AuditId { get; private set; }

    public DateTime DetectedAtUtc { get; private set; }

    private OpenCriticalFinding()
    {
    }

    public static OpenCriticalFinding Create(Guid findingId, Guid auditId) =>
        new()
        {
            FindingId = findingId,
            AuditId = auditId,
            DetectedAtUtc = DateTime.UtcNow,
        };
}
