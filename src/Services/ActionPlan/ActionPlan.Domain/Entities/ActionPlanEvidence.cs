using Ehs.SharedKernel.Domain;

namespace ActionPlan.Domain.Entities;

/// <summary>
/// Metadata only, per CLAUDE.md §10: "store only metadata/blob references in
/// SQL, with generated unique blob names." <see cref="BlobReference"/> is
/// that opaque reference — this service never touches the file bytes
/// themselves. Mirrors Finding Service's FindingDocument.
/// </summary>
public sealed class ActionPlanEvidence : AuditableEntity<Guid>
{
    public Guid ActionPlanId { get; private set; }

    public string FileName { get; private set; } = default!;

    public string BlobReference { get; private set; } = default!;

    public string ContentType { get; private set; } = default!;

    public long SizeBytes { get; private set; }

    private ActionPlanEvidence()
    {
    }

    internal static ActionPlanEvidence Create(Guid actionPlanId, string fileName, string blobReference, string contentType, long sizeBytes, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            ActionPlanId = actionPlanId,
            FileName = fileName,
            BlobReference = blobReference,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
}
