using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Entities;

public sealed class ActionPlanComment : AuditableEntity<Guid>
{
    public Guid ActionPlanId { get; private set; }

    public string AuthorId { get; private set; } = default!;

    public string AuthorName { get; private set; } = default!;

    public string Text { get; private set; } = default!;

    private ActionPlanComment()
    {
    }

    internal static ActionPlanComment Create(Guid actionPlanId, string authorId, string authorName, string text, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            ActionPlanId = actionPlanId,
            AuthorId = authorId,
            AuthorName = authorName,
            Text = text,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
}
