namespace Finding.Contracts.Dtos;

public sealed record FindingStatusHistoryDto(Guid Id, string? FromStatus, string ToStatus, string ChangedBy, DateTime ChangedAtUtc);
