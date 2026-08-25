using Ehs.SharedKernel.Domain;

namespace Audit.Domain.Entities;

public sealed class ChecklistQuestion : AuditableEntity<Guid>
{
    public Guid ChecklistId { get; private set; }

    public string Text { get; private set; } = default!;

    public bool IsMandatory { get; private set; }

    public int DisplayOrder { get; private set; }

    private ChecklistQuestion()
    {
    }

    internal static ChecklistQuestion Create(Guid checklistId, string text, bool isMandatory, int displayOrder, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            ChecklistId = checklistId,
            Text = text,
            IsMandatory = isMandatory,
            DisplayOrder = displayOrder,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
}
