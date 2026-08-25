using Audit.Domain.Enums;
using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Entities;

/// <summary>
/// An auditor or auditee assigned to an audit. <see cref="UserId"/> is the
/// Entra object id; identity/role enforcement against the caller's JWT
/// happens at the Api layer in a later phase, not here.
/// </summary>
public sealed class AuditTeamMember : AuditableEntity<Guid>
{
    public Guid AuditId { get; private set; }

    public string UserId { get; private set; } = default!;

    public string DisplayName { get; private set; } = default!;

    public AuditTeamRole Role { get; private set; }

    private AuditTeamMember()
    {
    }

    internal static AuditTeamMember Create(Guid auditId, string userId, string displayName, AuditTeamRole role, string createdBy)
    {
        var member = new AuditTeamMember
        {
            Id = Guid.NewGuid(),
            AuditId = auditId,
            UserId = userId,
            DisplayName = displayName,
            Role = role,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
        return member;
    }
}
