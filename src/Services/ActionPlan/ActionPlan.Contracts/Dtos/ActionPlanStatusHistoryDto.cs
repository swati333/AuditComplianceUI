namespace ActionPlan.Contracts.Dtos;

public sealed record ActionPlanStatusHistoryDto(Guid Id, string? FromStatus, string ToStatus, string ChangedBy, DateTime ChangedAtUtc, string? Reason);
