using Ehs.SharedKernel.Domain;
using Ehs.SharedKernel.Exceptions;
using Notification.Domain.Enums;

namespace Notification.Domain.Entities;

/// <summary>
/// One rendered notification addressed to (at most) one recipient over one
/// channel. Owns its own delivery state machine — retry count, next-retry
/// timestamp (exponential backoff) and dead-lettering after
/// <see cref="MaxRetryCount"/> failures — independent of the source event
/// that triggered it (that's only recorded for traceability via
/// <see cref="CorrelationId"/>/<see cref="SourceEventType"/>).
/// </summary>
public sealed class Notification : AuditableEntity<Guid>
{
    public const int MaxRetryCount = 5;
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Null when the source event carried no resolvable recipient (see
    /// AuditPlannedHandler/CriticalFindingCreatedHandler) — a broadcast
    /// notification, currently InApp-only since there's no address to email.
    /// </summary>
    public string? RecipientUserId { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public string TemplateCode { get; private set; } = default!;

    public string Subject { get; private set; } = default!;

    public string Body { get; private set; } = default!;

    public NotificationStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public DateTime? NextRetryAtUtc { get; private set; }

    public string? LastError { get; private set; }

    public DateTime? SentAtUtc { get; private set; }

    public bool IsRead { get; private set; }

    public DateTime? ReadAtUtc { get; private set; }

    public Guid CorrelationId { get; private set; }

    public string SourceEventType { get; private set; } = default!;

    private Notification()
    {
    }

    public static Notification Create(
        string? recipientUserId,
        NotificationChannel channel,
        string templateCode,
        string subject,
        string body,
        Guid correlationId,
        string sourceEventType,
        string createdBy)
    {
        var now = DateTime.UtcNow;
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = recipientUserId,
            Channel = channel,
            TemplateCode = templateCode,
            Subject = subject,
            Body = body,
            CorrelationId = correlationId,
            SourceEventType = sourceEventType,
            CreatedBy = createdBy,
            CreatedDate = now,
        };

        // InApp has nothing external to deliver: the moment it's persisted
        // it's visible to the frontend, so it's already "sent".
        if (channel == NotificationChannel.InApp)
        {
            notification.Status = NotificationStatus.Sent;
            notification.SentAtUtc = now;
        }
        else
        {
            notification.Status = NotificationStatus.Pending;
        }

        return notification;
    }

    public void MarkSent(string modifiedBy)
    {
        Status = NotificationStatus.Sent;
        SentAtUtc = DateTime.UtcNow;
        NextRetryAtUtc = null;
        LastError = null;
        Touch(modifiedBy);
    }

    /// <summary>
    /// Exponential backoff: 30s, 60s, 120s, 240s, 480s. On the
    /// <see cref="MaxRetryCount"/>th failure there is no further retry —
    /// the notification is dead-lettered (recorded, not deleted, for
    /// investigation) rather than retried forever.
    /// </summary>
    public void RecordDeliveryFailure(string error, string modifiedBy)
    {
        RetryCount++;
        LastError = error;

        if (RetryCount >= MaxRetryCount)
        {
            Status = NotificationStatus.DeadLettered;
            NextRetryAtUtc = null;
        }
        else
        {
            Status = NotificationStatus.Failed;
            NextRetryAtUtc = DateTime.UtcNow.Add(TimeSpan.FromSeconds(BaseRetryDelay.TotalSeconds * Math.Pow(2, RetryCount - 1)));
        }

        Touch(modifiedBy);
    }

    public void MarkRead(string modifiedBy)
    {
        if (Channel != NotificationChannel.InApp)
        {
            throw new ConflictException("Only in-app notifications can be marked read.", "NOT_AN_INAPP_NOTIFICATION");
        }

        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAtUtc = DateTime.UtcNow;
        Touch(modifiedBy);
    }

    private void Touch(string modifiedBy)
    {
        ModifiedBy = modifiedBy;
        ModifiedDate = DateTime.UtcNow;
    }
}
