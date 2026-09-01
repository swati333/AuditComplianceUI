namespace Audit.Domain.Entities;

/// <summary>
/// Local projection of "this audit has a mandatory corrective action that is
/// not yet closed or cancelled" — populated exclusively by consuming Action
/// Plan Service's ActionPlanAssigned integration event (row added, only when
/// <see cref="FindingReference"/> shows the owning finding is High/Critical)
/// and ActionPlanStatusChanged with ToStatus Closed or Cancelled (row
/// removed), never by querying ActionPlanDb (CLAUDE.md §5). Backs
/// <c>IAuditComplianceGateway.GetComplianceStatusAsync</c>'s
/// HasOpenRequiredActions answer, which Audit.Close() uses to enforce "an
/// audit cannot close while mandatory actions ... are open" (CLAUDE.md §2).
/// </summary>
public sealed class OpenRequiredAction
{
    public Guid ActionPlanId { get; private set; }

    public Guid FindingId { get; private set; }

    public Guid AuditId { get; private set; }

    public DateTime AssignedAtUtc { get; private set; }

    private OpenRequiredAction()
    {
    }

    public static OpenRequiredAction Create(Guid actionPlanId, Guid findingId, Guid auditId) =>
        new()
        {
            ActionPlanId = actionPlanId,
            FindingId = findingId,
            AuditId = auditId,
            AssignedAtUtc = DateTime.UtcNow,
        };
}
