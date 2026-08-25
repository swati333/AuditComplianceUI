namespace Finding.Contracts.Dtos;

public sealed record FindingSummaryDto(
    Guid Id,
    Guid AuditId,
    string Title,
    string Severity,
    string Status,
    DateTime CreatedDate,
    DateTime? ModifiedDate);
