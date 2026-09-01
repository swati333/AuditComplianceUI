using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Exceptions;
using Finding.Domain.Enums;
using Finding.Domain.Events;

namespace Finding.Domain.Entities;

/// <summary>
/// Aggregate root for a single compliance gap raised during an audit.
/// References its owning audit only by <see cref="AuditId"/> — Finding
/// Service never joins across to AuditDb (CLAUDE.md §5). All state
/// transitions go through named methods so every business rule in
/// CLAUDE.md §2 is enforced in exactly one place.
/// </summary>
public sealed class Finding : AuditableEntity<Guid>
{
    private readonly List<FindingComment> _comments = [];
    private readonly List<FindingDocument> _documents = [];
    private readonly List<FindingStatusHistory> _statusHistory = [];

    public Guid AuditId { get; private set; }

    public string Title { get; private set; } = default!;

    public string? Description { get; private set; }

    public FindingSeverity Severity { get; private set; }

    public FindingStatus Status { get; private set; }

    public string? RootCauseAnalysis { get; private set; }

    public string? RootCauseAnalysisBy { get; private set; }

    public DateTime? RootCauseAnalysisAtUtc { get; private set; }

    public IReadOnlyCollection<FindingComment> Comments => _comments.AsReadOnly();

    public IReadOnlyCollection<FindingDocument> Documents => _documents.AsReadOnly();

    public IReadOnlyCollection<FindingStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Finding()
    {
    }

    public static Finding Create(Guid auditId, string title, string? description, FindingSeverity severity, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["title"] = ["Title is required."],
            });
        }

        var now = DateTime.UtcNow;
        var finding = new Finding
        {
            Id = Guid.NewGuid(),
            AuditId = auditId,
            Title = title,
            Description = description,
            Severity = severity,
            Status = FindingStatus.Open,
            CreatedBy = createdBy,
            CreatedDate = now,
        };

        finding._statusHistory.Add(FindingStatusHistory.Create(finding.Id, null, FindingStatus.Open, createdBy));
        finding.AddDomainEvent(new FindingCreatedDomainEvent(finding.Id, auditId, title, severity, now));

        if (severity == FindingSeverity.Critical)
        {
            finding.AddDomainEvent(new CriticalFindingCreatedDomainEvent(finding.Id, auditId, title, now));
        }

        return finding;
    }

    public void UpdateDetails(string title, string? description, FindingSeverity severity, string modifiedBy)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["title"] = ["Title is required."],
            });
        }

        Title = title;
        Description = description;
        Severity = severity;
        Touch(modifiedBy);
    }

    public void RecordRootCauseAnalysis(string text, string modifiedBy)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["text"] = ["Root cause analysis text is required."],
            });
        }

        RootCauseAnalysis = text;
        RootCauseAnalysisBy = modifiedBy;
        RootCauseAnalysisAtUtc = DateTime.UtcNow;
        Touch(modifiedBy);
    }

    public FindingComment AddComment(string authorId, string authorName, string text, string modifiedBy)
    {
        EnsureEditable();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["text"] = ["Comment text is required."],
            });
        }

        var comment = FindingComment.Create(Id, authorId, authorName, text, modifiedBy);
        _comments.Add(comment);
        Touch(modifiedBy);
        return comment;
    }

    public FindingDocument AddDocument(string fileName, string blobReference, string contentType, long sizeBytes, string modifiedBy)
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

        var document = FindingDocument.Create(Id, fileName, blobReference, contentType, sizeBytes, modifiedBy);
        _documents.Add(document);
        Touch(modifiedBy);
        return document;
    }

    public void StartReview(string modifiedBy)
    {
        TransitionTo(FindingStatus.UnderReview, modifiedBy);
    }

    public void RequireAction(string modifiedBy)
    {
        TransitionTo(FindingStatus.ActionRequired, modifiedBy);
    }

    /// <summary>
    /// <paramref name="hasCorrectiveAction"/> is supplied by the Application
    /// layer via <c>IActionPlanGateway</c>, backed by a local read model kept
    /// in sync from Action Plan Service's ActionPlanAssigned integration
    /// event (see Finding.Infrastructure's ActionPlanGateway). The CLAUDE.md
    /// §2 rule itself — High/Critical findings require at least one
    /// corrective action before they can resolve — is fully enforced here.
    /// </summary>
    public void Resolve(bool hasCorrectiveAction, string modifiedBy)
    {
        EnsureTransitionAllowed(FindingStatus.Resolved);

        if (Severity is FindingSeverity.High or FindingSeverity.Critical && !hasCorrectiveAction)
        {
            throw new ConflictException(
                "High and Critical findings require at least one corrective action before they can be resolved.",
                "CORRECTIVE_ACTION_REQUIRED");
        }

        var now = DateTime.UtcNow;
        TransitionTo(FindingStatus.Resolved, modifiedBy);
        AddDomainEvent(new FindingResolvedDomainEvent(Id, AuditId, now));
    }

    public void Verify(string modifiedBy)
    {
        TransitionTo(FindingStatus.Verified, modifiedBy);
    }

    public void Close(string modifiedBy)
    {
        EnsureTransitionAllowed(FindingStatus.Closed);

        var now = DateTime.UtcNow;
        TransitionTo(FindingStatus.Closed, modifiedBy);
        AddDomainEvent(new FindingClosedDomainEvent(Id, AuditId, now));
    }

    public void SoftDelete(string modifiedBy)
    {
        IsDeleted = true;
        Touch(modifiedBy);
    }

    private void EnsureEditable()
    {
        if (Status == FindingStatus.Closed)
        {
            throw new ConflictException("Finding cannot be modified once Closed.", "FINDING_NOT_EDITABLE");
        }
    }

    private void EnsureTransitionAllowed(FindingStatus target)
    {
        if (!FindingStatusTransitions.IsAllowed(Status, target))
        {
            throw new ConflictException($"Cannot transition finding from {Status} to {target}.", "INVALID_STATUS_TRANSITION");
        }
    }

    private void TransitionTo(FindingStatus target, string changedBy)
    {
        EnsureTransitionAllowed(target);
        var from = Status;
        Status = target;
        _statusHistory.Add(FindingStatusHistory.Create(Id, from, target, changedBy));
        Touch(changedBy);
    }

    private void Touch(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
