namespace Notification.Domain.Enums;

/// <summary>
/// InApp notifications go straight to Sent at creation (there's nothing
/// external to deliver — the frontend reads NotificationDb directly). Email
/// starts Pending and moves through Failed (awaiting retry) to either Sent
/// or, after the retry budget is exhausted, DeadLettered.
/// </summary>
public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    DeadLettered = 3,
}
