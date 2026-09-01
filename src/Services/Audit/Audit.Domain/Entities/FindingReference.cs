namespace Audit.Domain.Entities;

/// <summary>
/// Local, eventually-consistent projection of the High/Critical findings
/// Audit Service knows about — populated exclusively by consuming Finding
/// Service's FindingCreated integration event, filtered to Severity High or
/// Critical, never by querying FindingDb (CLAUDE.md §5). Exists only to
/// resolve FindingId → AuditId when a later ActionPlanAssigned/
/// ActionPlanStatusChanged event (which carries FindingId but not AuditId)
/// needs to be attributed to an audit for <see cref="OpenRequiredAction"/>
/// tracking — Low/Medium findings are never stored here because only
/// High/Critical findings can carry a "mandatory" action (CLAUDE.md §2:
/// "High/Critical findings require at least one corrective action").
/// </summary>
public sealed class FindingReference
{
    public Guid FindingId { get; private set; }

    public Guid AuditId { get; private set; }

    public string Severity { get; private set; } = default!;

    public DateTime CreatedAtUtc { get; private set; }

    private FindingReference()
    {
    }

    public static FindingReference Create(Guid findingId, Guid auditId, string severity) =>
        new()
        {
            FindingId = findingId,
            AuditId = auditId,
            Severity = severity,
            CreatedAtUtc = DateTime.UtcNow,
        };
}
