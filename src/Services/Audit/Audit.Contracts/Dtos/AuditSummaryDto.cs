namespace Audit.Contracts.Dtos;

/// <summary>Lightweight projection for the audit list endpoint.</summary>
public sealed record AuditSummaryDto(
    Guid Id,
    string Title,
    string Scope,
    string Location,
    string Status,
    DateTime? PlannedStartDate,
    DateTime? PlannedEndDate,
    DateTime CreatedDate,
    DateTime? ModifiedDate);
