namespace Finding.Contracts.Requests;

/// <summary>
/// <see cref="BlobReference"/> is the already-uploaded blob's opaque
/// name/URI (CLAUDE.md §10: short-lived SAS/API-mediated upload happens
/// client-side or via a separate endpoint; this call only records metadata).
/// </summary>
public sealed record AddDocumentRequest(string FileName, string BlobReference, string ContentType, long SizeBytes);
