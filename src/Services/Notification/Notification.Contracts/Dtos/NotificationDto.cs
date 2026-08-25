namespace Notification.Contracts.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string? RecipientUserId,
    string Channel,
    string TemplateCode,
    string Subject,
    string Body,
    string Status,
    int RetryCount,
    DateTime? NextRetryAtUtc,
    string? LastError,
    DateTime? SentAtUtc,
    bool IsRead,
    DateTime? ReadAtUtc,
    Guid CorrelationId,
    string SourceEventType,
    DateTime CreatedDate);
