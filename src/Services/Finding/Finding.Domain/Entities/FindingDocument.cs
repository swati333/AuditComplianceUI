using Ehs.SharedKernel.Domain;

namespace Finding.Domain.Entities;

/// <summary>
/// Metadata only, per CLAUDE.md §10: "store only metadata/blob references in
/// SQL, with generated unique blob names." <see cref="BlobReference"/> is
/// that opaque reference (e.g. a blob name/URI) — this service never touches
/// the file bytes themselves.
/// </summary>
public sealed class FindingDocument : AuditableEntity<Guid>
{
    public Guid FindingId { get; private set; }

    public string FileName { get; private set; } = default!;

    public string BlobReference { get; private set; } = default!;

    public string ContentType { get; private set; } = default!;

    public long SizeBytes { get; private set; }

    private FindingDocument()
    {
    }

    internal static FindingDocument Create(Guid findingId, string fileName, string blobReference, string contentType, long sizeBytes, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            FindingId = findingId,
            FileName = fileName,
            BlobReference = blobReference,
            ContentType = contentType,
            SizeBytes = sizeBytes,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };
}
