using Audit.Domain.Enums;
using Audit.Domain.Events;
using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Exceptions;

namespace Audit.Domain.Entities;

/// <summary>
/// Aggregate root for a single planned/executed inspection. Owns its team
/// assignments, checklist responses and status history; references a
/// <see cref="Checklist"/> by id only (separate aggregate, per CLAUDE.md §5
/// "no cross-aggregate/database foreign keys" spirit even within one service).
/// All state transitions go through named methods so every business rule in
/// CLAUDE.md §2 is enforced in exactly one place.
/// </summary>
public sealed class Audit : AuditableEntity<Guid>
{
    private readonly List<AuditTeamMember> _teamMembers = [];
    private readonly List<ChecklistResponse> _checklistResponses = [];
    private readonly List<AuditStatusHistory> _statusHistory = [];

    public string Title { get; private set; } = default!;

    public string? Description { get; private set; }

    public string Scope { get; private set; } = default!;

    public string Location { get; private set; } = default!;

    public AuditStatus Status { get; private set; }

    public Guid? ChecklistId { get; private set; }

    public DateTime? PlannedStartDate { get; private set; }

    public DateTime? PlannedEndDate { get; private set; }

    public DateTime? ActualStartDate { get; private set; }

    public DateTime? ActualEndDate { get; private set; }

    public string? CancellationReason { get; private set; }

    public IReadOnlyCollection<AuditTeamMember> TeamMembers => _teamMembers.AsReadOnly();

    public IReadOnlyCollection<ChecklistResponse> ChecklistResponses => _checklistResponses.AsReadOnly();

    public IReadOnlyCollection<AuditStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Audit()
    {
    }

    public static Audit Create(string title, string? description, string scope, string location, string createdBy)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Title is required."];
        }

        if (string.IsNullOrWhiteSpace(scope))
        {
            errors["scope"] = ["Scope is required."];
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            errors["location"] = ["Location is required."];
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }

        var now = DateTime.UtcNow;
        var audit = new Audit
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Scope = scope,
            Location = location,
            Status = AuditStatus.Draft,
            CreatedBy = createdBy,
            CreatedDate = now,
        };

        audit._statusHistory.Add(AuditStatusHistory.Create(audit.Id, null, AuditStatus.Draft, createdBy));
        audit.AddDomainEvent(new AuditCreatedDomainEvent(audit.Id, title, scope, location, now));

        return audit;
    }

    public void UpdateDetails(string title, string? description, string scope, string location, string modifiedBy)
    {
        EnsureEditable();

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Title is required."];
        }

        if (string.IsNullOrWhiteSpace(scope))
        {
            errors["scope"] = ["Scope is required."];
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            errors["location"] = ["Location is required."];
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }

        Title = title;
        Description = description;
        Scope = scope;
        Location = location;
        Touch(modifiedBy);
    }

    public void AssignChecklist(Guid checklistId, string modifiedBy)
    {
        EnsureEditable();
        ChecklistId = checklistId;
        Touch(modifiedBy);
    }

    public AuditTeamMember AssignTeamMember(string userId, string displayName, AuditTeamRole role, string modifiedBy)
    {
        EnsureEditable();

        if (_teamMembers.Any(m => m.UserId == userId && m.Role == role))
        {
            throw new ConflictException($"'{userId}' is already assigned to this audit as {role}.", "TEAM_MEMBER_ALREADY_ASSIGNED");
        }

        var member = AuditTeamMember.Create(Id, userId, displayName, role, modifiedBy);
        _teamMembers.Add(member);
        Touch(modifiedBy);
        return member;
    }

    public void RemoveTeamMember(Guid teamMemberId, string modifiedBy)
    {
        EnsureEditable();

        var member = _teamMembers.FirstOrDefault(m => m.Id == teamMemberId) ??
            throw new NotFoundException($"Team member '{teamMemberId}' was not found on this audit.");

        _teamMembers.Remove(member);
        Touch(modifiedBy);
    }

    public ChecklistResponse RecordChecklistResponse(Guid checklistQuestionId, string answerText, bool? isCompliant, string modifiedBy)
    {
        if (Status != AuditStatus.InProgress)
        {
            throw new ConflictException("Checklist responses can only be recorded while the audit is in progress.", "AUDIT_NOT_IN_PROGRESS");
        }

        if (string.IsNullOrWhiteSpace(answerText))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["answerText"] = ["Answer text is required."],
            });
        }

        var existing = _checklistResponses.FirstOrDefault(r => r.ChecklistQuestionId == checklistQuestionId);
        if (existing is not null)
        {
            existing.UpdateAnswer(answerText, isCompliant, modifiedBy);
            Touch(modifiedBy);
            return existing;
        }

        var response = ChecklistResponse.Create(Id, checklistQuestionId, answerText, isCompliant, modifiedBy);
        _checklistResponses.Add(response);
        Touch(modifiedBy);
        return response;
    }

    public void Plan(DateTime plannedStartDate, DateTime plannedEndDate, string modifiedBy)
    {
        EnsureTransitionAllowed(AuditStatus.Planned);

        if (string.IsNullOrWhiteSpace(Scope) || string.IsNullOrWhiteSpace(Location))
        {
            throw new ConflictException("Audit needs scope and location before it can be planned.", "AUDIT_MISSING_SCOPE_OR_LOCATION");
        }

        if (_teamMembers.Count == 0)
        {
            throw new ConflictException("Audit needs an assigned team before it can be planned.", "AUDIT_TEAM_NOT_ASSIGNED");
        }

        if (plannedEndDate < plannedStartDate)
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["plannedEndDate"] = ["Planned end date must be on or after the planned start date."],
            });
        }

        PlannedStartDate = plannedStartDate;
        PlannedEndDate = plannedEndDate;
        TransitionTo(AuditStatus.Planned, modifiedBy);
        AddDomainEvent(new AuditPlannedDomainEvent(Id, plannedStartDate, plannedEndDate));
    }

    public void Start(string modifiedBy)
    {
        EnsureTransitionAllowed(AuditStatus.InProgress);

        var now = DateTime.UtcNow;
        ActualStartDate = now;
        TransitionTo(AuditStatus.InProgress, modifiedBy);
        AddDomainEvent(new AuditStartedDomainEvent(Id, now));
    }

    /// <summary>
    /// <paramref name="mandatoryQuestionIds"/> is supplied by the Application
    /// layer (loaded from the assigned Checklist aggregate) so this aggregate
    /// never needs a direct reference/navigation to Checklist.
    /// </summary>
    public void Complete(IReadOnlyCollection<Guid> mandatoryQuestionIds, string modifiedBy)
    {
        EnsureTransitionAllowed(AuditStatus.Completed);

        var answeredQuestionIds = _checklistResponses
            .Where(r => r.HasAnswer())
            .Select(r => r.ChecklistQuestionId)
            .ToHashSet();

        var unanswered = mandatoryQuestionIds.Where(id => !answeredQuestionIds.Contains(id)).ToList();
        if (unanswered.Count > 0)
        {
            throw new ConflictException(
                $"{unanswered.Count} mandatory checklist question(s) are unanswered.",
                "MANDATORY_QUESTIONS_UNANSWERED");
        }

        var now = DateTime.UtcNow;
        ActualEndDate = now;
        TransitionTo(AuditStatus.Completed, modifiedBy);
        AddDomainEvent(new AuditCompletedDomainEvent(Id, now));
    }

    /// <summary>
    /// <paramref name="hasOpenCriticalFindings"/>/<paramref name="hasOpenRequiredActions"/>
    /// are supplied by the Application layer via a compliance-status port,
    /// backed by a local read model synced from Finding/Action Plan Service
    /// events (see Audit.Infrastructure's AuditComplianceGateway).
    /// </summary>
    public void Close(bool hasOpenCriticalFindings, bool hasOpenRequiredActions, string modifiedBy)
    {
        EnsureTransitionAllowed(AuditStatus.Closed);

        if (hasOpenCriticalFindings)
        {
            throw new ConflictException("Audit cannot close while Critical findings remain open.", "OPEN_CRITICAL_FINDINGS");
        }

        if (hasOpenRequiredActions)
        {
            throw new ConflictException("Audit cannot close while required actions remain open.", "OPEN_REQUIRED_ACTIONS");
        }

        TransitionTo(AuditStatus.Closed, modifiedBy);
        AddDomainEvent(new AuditClosedDomainEvent(Id, DateTime.UtcNow));
    }

    public void Cancel(string? reason, string modifiedBy)
    {
        EnsureTransitionAllowed(AuditStatus.Cancelled);
        CancellationReason = reason;
        TransitionTo(AuditStatus.Cancelled, modifiedBy, reason);
    }

    public void SoftDelete(string modifiedBy)
    {
        IsDeleted = true;
        Touch(modifiedBy);
    }

    private void EnsureEditable()
    {
        if (Status is AuditStatus.Closed or AuditStatus.Cancelled)
        {
            throw new ConflictException($"Audit cannot be modified while it is {Status}.", "AUDIT_NOT_EDITABLE");
        }
    }

    private void EnsureTransitionAllowed(AuditStatus target)
    {
        if (!AuditStatusTransitions.IsAllowed(Status, target))
        {
            throw new ConflictException($"Cannot transition audit from {Status} to {target}.", "INVALID_STATUS_TRANSITION");
        }
    }

    private void TransitionTo(AuditStatus target, string changedBy, string? reason = null)
    {
        var from = Status;
        Status = target;
        _statusHistory.Add(AuditStatusHistory.Create(Id, from, target, changedBy, reason));
        Touch(changedBy);
    }

    private void Touch(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
