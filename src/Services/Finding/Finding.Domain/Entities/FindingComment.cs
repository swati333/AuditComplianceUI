using Ehs.SharedKernel.Domain;

namespace Finding.Domain.Entities;

public sealed class FindingComment : AuditableEntity<Guid>
{
    public Guid FindingId { get; private set; }

    public string AuthorId { get; private set; } = default!;

    public string AuthorName { get; private set; } = default!;

    public string Text { get; private set; } = default!;

    private FindingComment()
    {
    }

    internal static FindingComment Create(Guid findingId, string authorId, string authorName, string text, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            FindingId = findingId,
            AuthorId = authorId,
            AuthorName = authorName,
            Text = text,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
}
