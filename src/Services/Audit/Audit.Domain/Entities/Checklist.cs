using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Exceptions;

namespace Audit.Domain.Entities;

/// <summary>
/// A reusable, configurable set of inspection questions (CLAUDE.md §2) that
/// can be assigned to any number of audits. Separate aggregate root from
/// <see cref="Audit"/> — an audit references a checklist by id, it does not
/// own its question definitions.
/// </summary>
public sealed class Checklist : AuditableEntity<Guid>
{
    private readonly List<ChecklistQuestion> _questions = [];

    public string Name { get; private set; } = default!;

    public string? Description { get; private set; }

    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<ChecklistQuestion> Questions => _questions.AsReadOnly();

    private Checklist()
    {
    }

    public static Checklist Create(string name, string? description, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["name"] = ["Checklist name is required."],
            });
        }

        return new Checklist
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
    }

    public ChecklistQuestion AddQuestion(string text, bool isMandatory, int displayOrder, string modifiedBy)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new BusinessValidationException(new Dictionary<string, string[]>
            {
                ["text"] = ["Question text is required."],
            });
        }

        var question = ChecklistQuestion.Create(Id, text, isMandatory, displayOrder, modifiedBy);
        _questions.Add(question);
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
        return question;
    }

    public IReadOnlyCollection<Guid> MandatoryQuestionIds() =>
        _questions.Where(q => q.IsMandatory).Select(q => q.Id).ToList();
}
