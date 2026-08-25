namespace Notification.Contracts.Dtos;

public sealed record NotificationTemplateDto(Guid Id, string Code, string Channel, string? SubjectTemplate, string BodyTemplate, bool IsActive);
