namespace ActionPlan.Contracts.Dtos;

public sealed record ActionPlanEvidenceDto(Guid Id, string FileName, string BlobReference, string ContentType, long SizeBytes, DateTime CreatedDate);
