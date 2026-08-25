using Notification.Contracts.Dtos;
using Notification.Domain.Entities;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Mapping;

public static class NotificationMappingExtensions
{
    public static NotificationDto ToDto(this NotificationEntity notification) =>
        new(
            notification.Id,
            notification.RecipientUserId,
            notification.Channel.ToString(),
            notification.TemplateCode,
            notification.Subject,
            notification.Body,
            notification.Status.ToString(),
            notification.RetryCount,
            notification.NextRetryAtUtc,
            notification.LastError,
            notification.SentAtUtc,
            notification.IsRead,
            notification.ReadAtUtc,
            notification.CorrelationId,
            notification.SourceEventType,
            notification.CreatedDate);

    public static NotificationTemplateDto ToDto(this NotificationTemplate template) =>
        new(template.Id, template.Code, template.Channel.ToString(), template.SubjectTemplate, template.BodyTemplate, template.IsActive);

    public static NotificationPreferenceDto ToDto(this NotificationPreference preference) =>
        new(preference.Channel.ToString(), preference.IsEnabled);
}
