namespace Finding.Contracts.Dtos;

public sealed record FindingDocumentDto(Guid Id, string FileName, string BlobReference, string ContentType, long SizeBytes, DateTime CreatedDate);
