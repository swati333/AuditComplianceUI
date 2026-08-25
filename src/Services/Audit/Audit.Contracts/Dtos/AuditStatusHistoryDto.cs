namespace Audit.Contracts.Dtos;

public sealed record AuditStatusHistoryDto(Guid Id, string? FromStatus, string ToStatus, string ChangedBy, DateTime ChangedAtUtc, string? Reason);
