using ActionPlan.Domain.Enums;
using ActionPlan.Domain.Events;
using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Exceptions;

namespace ActionPlan.Domain.Entities;

/// <summary>
/// Aggregate root for a single corrective/preventive action tied to a
/// finding. References its owning finding only by <see cref="FindingId"/> —
/// Action Plan Service never joins across to FindingDb (CLAUDE.md §5). All
/// state transitions go through named methods so every business rule in
/// CLAUDE.md §2 (self-approval prevention, rejected-returns-to-in-progress,
/// overdue escalation) is enforced in exactly one place.
/// </summary>
public sealed class ActionPlan : AuditableEntity<Guid>
{
    private readonly List<ActionPlanComment> _comments = [];
    private readonly List<ActionPlanEvidence> _evidence = [];
    private readonly List<ActionPlanStatusHistory> _statusHistory = [];

    public Guid FindingId { get; private set; }

    public ActionType ActionType { get; private set; }

    public string Title { get; private set; } = default!;

    public string? Description { get; private set; }

    public string OwnerId { get; private set; } = default!;

    public string OwnerName { get; private set; } = default!;

    public string ApproverId { get; private set; } = default!;

    public string ApproverName { get; private set; } = default!;

    public DateTime DueDate { get; private set; }

    public ActionPriority Priority { get; private set; }

    public ActionPlanStatus Status { get; private set; }

    public string? RejectionReason { get; private set; }

    public IReadOnlyCollection<ActionPlanComment> Comments => _comments.AsReadOnly();

    public IReadOnlyCollection<ActionPlanEvidence> Evidence => _evidence.AsReadOnly();

    public IReadOnlyCollection<ActionPlanStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private ActionPlan()
    {
    }

    public static ActionPlan Create(
        Guid findingId,
        ActionType actionType,
        string title,
        string? description,
        string ownerId,
        string ownerName,
        string approverId,
        string approverName,
        DateTime dueDate,
        ActionPriority priority,
        string createdBy)
    {
        ValidateAssignment(title, ownerId, approverId);

        var now = DateTime.UtcNow;
        var actionPlan = new ActionPlan
        {
            Id = Guid.NewGuid(),
            FindingId = findingId,
            ActionType = actionType,
            Title = title,
            Description = description,
            OwnerId = ownerId,
            OwnerName = ownerName,
            ApproverId = approverId,
            ApproverName = approverName,
            DueDate = dueDate,
            Priority = priority,
            Status = ActionPlanStatus.Assigned,
            CreatedBy = createdBy,
            CreatedDate = now,
        };

        actionPlan._statusHistory.Add(ActionPlanStatusHistory.Create(actionPlan.Id, null, ActionPlanStatus.Assigned, createdBy));
        actionPlan.AddDomainEvent(new ActionPlanAssignedDomainEvent(actionPlan.Id, findingId, ownerId, approverId, dueDate, priority, now));

        return actionPlan;
    }

    public void UpdateDetails(
        ActionType actionType,
        string title,
        string? description,
        string ownerId,
        string ownerName,
        string approverId,
        string approverName,
        DateTime dueDate,
        ActionPriority priority,
        string modifiedBy)
    {
        EnsureEditable();
        ValidateAssignment(title, ownerId, approverId);

        ActionType = actionType;
        Title = title;
        Description = description;
        OwnerId = ownerId;
        OwnerName = ownerName;
        ApproverId = approverId;
        ApproverName = approverName;
        DueDate = dueDate;
        Priority = priority;
        Touch(modifiedBy);
    }

    public ActionPlanComment AddComment(string authorId, string authorName, string text, string modifiedBy)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["text"] = ["Comment text is required."],
            });
        }

        var comment = ActionPlanComment.Create(Id, authorId, authorName, text, modifiedBy);
        _comments.Add(comment);
        Touch(modifiedBy);
        return comment;
    }

    public ActionPlanEvidence AddEvidence(string fileName, string blobReference, string contentType, long sizeBytes, string modifiedBy)
    {
        EnsureEditable();

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            errors["fileName"] = ["File name is required."];
        }

        if (string.IsNullOrWhiteSpace(blobReference))
        {
            errors["blobReference"] = ["Blob reference is required."];
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }

        var evidence = ActionPlanEvidence.Create(Id, fileName, blobReference, contentType, sizeBytes, modifiedBy);
        _evidence.Add(evidence);
        Touch(modifiedBy);
        return evidence;
    }

    /// <summary>
    /// Begins (Assigned→InProgress) or resumes (Rejected/Overdue→InProgress,
    /// CLAUDE.md §2's "Rejected actions return to In Progress") work on the
    /// action. <paramref name="currentUserId"/> must be the assigned owner —
    /// CanManageOwnActions only proves the caller owns *some* actions, not
    /// this one, so the aggregate itself enforces which action plan.
    /// </summary>
    public void Start(string currentUserId, string modifiedBy)
    {
        EnsureIsOwner(currentUserId);
        TransitionTo(ActionPlanStatus.InProgress, modifiedBy);
    }

    /// <summary><paramref name="currentUserId"/> must be the assigned owner — see <see cref="Start"/>.</summary>
    public void Submit(string currentUserId, string modifiedBy)
    {
        EnsureIsOwner(currentUserId);

        var now = DateTime.UtcNow;
        TransitionTo(ActionPlanStatus.SubmittedForApproval, modifiedBy);
        AddDomainEvent(new ActionPlanSubmittedDomainEvent(Id, FindingId, ApproverId, now));
    }

    /// <summary><paramref name="currentUserId"/> is the authenticated caller, checked against <see cref="OwnerId"/> — self-approval prevention (CLAUDE.md §2), enforced here as well as at assignment time (defense in depth).</summary>
    public void Approve(string currentUserId, string modifiedBy)
    {
        EnsureNotSelfApproval(currentUserId);

        var now = DateTime.UtcNow;
        TransitionTo(ActionPlanStatus.Approved, modifiedBy);
        AddDomainEvent(new ActionPlanApprovedDomainEvent(Id, FindingId, currentUserId, now));
    }

    public void Reject(string currentUserId, string reason, string modifiedBy)
    {
        EnsureNotSelfApproval(currentUserId);

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["reason"] = ["A rejection reason is required."],
            });
        }

        var now = DateTime.UtcNow;
        EnsureTransitionAllowed(ActionPlanStatus.Rejected);
        var from = Status;
        Status = ActionPlanStatus.Rejected;
        RejectionReason = reason;
        _statusHistory.Add(ActionPlanStatusHistory.Create(Id, from, ActionPlanStatus.Rejected, modifiedBy, reason));
        Touch(modifiedBy);
        AddDomainEvent(new ActionPlanRejectedDomainEvent(Id, FindingId, currentUserId, reason, now));
    }

    public void Close(string modifiedBy)
    {
        TransitionTo(ActionPlanStatus.Closed, modifiedBy);
    }

    public void Cancel(string modifiedBy)
    {
        TransitionTo(ActionPlanStatus.Cancelled, modifiedBy);
    }

    /// <summary>
    /// System-only transition (CLAUDE.md §2: "unclosed actions past due date
    /// become Overdue and escalate") — invoked by
    /// Infrastructure.OverdueDetectionService, never by a controller action.
    /// A no-op if the action isn't currently eligible (already
    /// submitted/approved/closed/cancelled, or not yet past due), so the
    /// caller can call this unconditionally for every candidate row.
    /// </summary>
    public bool MarkOverdue(DateTime utcNow)
    {
        if (!ActionPlanStatusTransitions.CanBecomeOverdue(Status) || DueDate >= utcNow)
        {
            return false;
        }

        var from = Status;
        Status = ActionPlanStatus.Overdue;
        _statusHistory.Add(ActionPlanStatusHistory.Create(Id, from, ActionPlanStatus.Overdue, "system"));
        Touch("system");
        AddDomainEvent(new ActionPlanOverdueDomainEvent(Id, FindingId, OwnerId, DueDate, utcNow));
        return true;
    }

    public void SoftDelete(string modifiedBy)
    {
        IsDeleted = true;
        Touch(modifiedBy);
    }

    private static void ValidateAssignment(string title, string ownerId, string approverId)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Title is required."];
        }

        if (string.IsNullOrWhiteSpace(ownerId))
        {
            errors["ownerId"] = ["Owner is required."];
        }

        if (string.IsNullOrWhiteSpace(approverId))
        {
            errors["approverId"] = ["Approver is required."];
        }

        if (!string.IsNullOrWhiteSpace(ownerId) && !string.IsNullOrWhiteSpace(approverId)
            && string.Equals(ownerId, approverId, StringComparison.OrdinalIgnoreCase))
        {
            errors["approverId"] = ["The approver cannot be the same person as the owner (self-approval is not allowed)."];
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }

    private void EnsureNotSelfApproval(string currentUserId)
    {
        if (string.Equals(currentUserId, OwnerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "An action owner cannot approve or reject their own action.",
                "SELF_APPROVAL_NOT_ALLOWED");
        }
    }

    private void EnsureIsOwner(string currentUserId)
    {
        if (!string.Equals(currentUserId, OwnerId, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException(
                "Only this action plan's assigned owner can perform this action.",
                "NOT_ACTION_OWNER");
        }
    }

    private void EnsureEditable()
    {
        if (Status is ActionPlanStatus.Closed or ActionPlanStatus.Cancelled)
        {
            throw new ConflictException($"Action plan cannot be modified once {Status}.", "ACTION_PLAN_NOT_EDITABLE");
        }
    }

    private void EnsureTransitionAllowed(ActionPlanStatus target)
    {
        if (!ActionPlanStatusTransitions.IsAllowed(Status, target))
        {
            throw new ConflictException($"Cannot transition action plan from {Status} to {target}.", "INVALID_STATUS_TRANSITION");
        }
    }

    private void TransitionTo(ActionPlanStatus target, string changedBy)
    {
        EnsureTransitionAllowed(target);
        var from = Status;
        Status = target;
        _statusHistory.Add(ActionPlanStatusHistory.Create(Id, from, target, changedBy));
        Touch(changedBy);

        if (target is ActionPlanStatus.InProgress or ActionPlanStatus.Closed or ActionPlanStatus.Cancelled)
        {
            AddDomainEvent(new ActionPlanStatusChangedDomainEvent(Id, FindingId, from, target, DateTime.UtcNow));
        }
    }

    private void Touch(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
