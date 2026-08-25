using Ehs.SharedKernel.Domain;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

/// <summary>
/// Per-(user, channel) opt-out. Absence of a row means "enabled" — the
/// default is opt-out, not opt-in, so notifications flow unless a user has
/// explicitly disabled a channel (see NotificationFactory).
/// </summary>
public sealed class NotificationPreference : AuditableEntity<Guid>
{
    public string UserId { get; private set; } = default!;

    public NotificationChannel Channel { get; private set; }

    public bool IsEnabled { get; private set; }

    private NotificationPreference()
    {
    }

    public static NotificationPreference Create(string userId, NotificationChannel channel, bool isEnabled, string createdBy) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Channel = channel,
            IsEnabled = isEnabled,
            CreatedBy = createdBy,
            CreatedDate = DateTime.UtcNow,
        };

    public void SetEnabled(bool isEnabled, string modifiedBy)
    {
        IsEnabled = isEnabled;
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
