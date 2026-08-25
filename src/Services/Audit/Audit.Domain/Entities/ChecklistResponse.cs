using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Entities;

/// <summary>
/// An answer recorded against one question of the audit's assigned checklist.
/// Lives inside the Audit aggregate (recorded during that audit's execution),
/// while the question definition itself belongs to the separate Checklist
/// aggregate (a checklist is a reusable template, not owned by one audit).
/// </summary>
public sealed class ChecklistResponse : AuditableEntity<Guid>
{
    public Guid AuditId { get; private set; }

    public Guid ChecklistQuestionId { get; private set; }

    public string AnswerText { get; private set; } = default!;

    public bool? IsCompliant { get; private set; }

    private ChecklistResponse()
    {
    }

    internal static ChecklistResponse Create(Guid auditId, Guid checklistQuestionId, string answerText, bool? isCompliant, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            AuditId = auditId,
            ChecklistQuestionId = checklistQuestionId,
            AnswerText = answerText,
            IsCompliant = isCompliant,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };

    internal void UpdateAnswer(string answerText, bool? isCompliant, string modifiedBy)
    {
        AnswerText = answerText;
        IsCompliant = isCompliant;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }

    public bool HasAnswer() => !string.IsNullOrWhiteSpace(AnswerText);
}
