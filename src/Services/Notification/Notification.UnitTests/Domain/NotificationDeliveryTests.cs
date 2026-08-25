using Ehs.SharedKernel.Exceptions;
using FluentAssertions;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.UnitTests.Domain;

/// <summary>Covers "Delivery status and retry count", "Exponential retry" and "Dead-letter handling" directly on the aggregate.</summary>
public class NotificationDeliveryTests
{
    private const string Actor = "system";

    private static NotificationEntity CreateEmailNotification() =>
        NotificationEntity.Create("user-1", NotificationChannel.Email, "Code", "Subject", "Body", Guid.NewGuid(), "SomeEvent", Actor);

    [Fact]
    public void MarkSent_sets_Sent_status_and_clears_retry_state()
    {
        var notification = CreateEmailNotification();
        notification.RecordDeliveryFailure("boom", Actor);

        notification.MarkSent(Actor);

        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAtUtc.Should().NotBeNull();
        notification.NextRetryAtUtc.Should().BeNull();
        notification.LastError.Should().BeNull();
    }

    [Fact]
    public void RecordDeliveryFailure_increments_RetryCount_and_records_the_error()
    {
        var notification = CreateEmailNotification();

        notification.RecordDeliveryFailure("SMTP timeout", Actor);

        notification.RetryCount.Should().Be(1);
        notification.LastError.Should().Be("SMTP timeout");
        notification.Status.Should().Be(NotificationStatus.Failed);
    }

    [Fact]
    public void RecordDeliveryFailure_schedules_exponentially_increasing_retry_delays()
    {
        var notification = CreateEmailNotification();
        var delays = new List<TimeSpan>();
        var previous = DateTime.UtcNow;

        for (var i = 0; i < NotificationEntity.MaxRetryCount - 1; i++)
        {
            notification.RecordDeliveryFailure($"failure {i}", Actor);
            delays.Add(notification.NextRetryAtUtc!.Value - previous);
        }

        // Each successive delay should be roughly double the previous one (30s, 60s, 120s, 240s).
        for (var i = 1; i < delays.Count; i++)
        {
            delays[i].TotalSeconds.Should().BeGreaterThan(delays[i - 1].TotalSeconds * 1.5);
        }
    }

    [Fact]
    public void RecordDeliveryFailure_dead_letters_after_MaxRetryCount_failures_instead_of_retrying_forever()
    {
        var notification = CreateEmailNotification();

        for (var i = 0; i < NotificationEntity.MaxRetryCount; i++)
        {
            notification.RecordDeliveryFailure($"failure {i}", Actor);
        }

        notification.Status.Should().Be(NotificationStatus.DeadLettered);
        notification.NextRetryAtUtc.Should().BeNull();
        notification.RetryCount.Should().Be(NotificationEntity.MaxRetryCount);
    }

    [Fact]
    public void MarkRead_is_rejected_for_Email_notifications()
    {
        var notification = CreateEmailNotification();

        var act = () => notification.MarkRead(Actor);

        act.Should().Throw<ConflictException>().Where(e => e.ErrorCode == "NOT_AN_INAPP_NOTIFICATION");
    }

    [Fact]
    public void MarkRead_sets_IsRead_for_InApp_notifications()
    {
        var notification = NotificationEntity.Create("user-1", NotificationChannel.InApp, "Code", "Subject", "Body", Guid.NewGuid(), "SomeEvent", Actor);

        notification.MarkRead(Actor);

        notification.IsRead.Should().BeTrue();
        notification.ReadAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void MarkRead_is_idempotent()
    {
        var notification = NotificationEntity.Create("user-1", NotificationChannel.InApp, "Code", "Subject", "Body", Guid.NewGuid(), "SomeEvent", Actor);
        notification.MarkRead(Actor);
        var firstReadAt = notification.ReadAtUtc;

        notification.MarkRead(Actor);

        notification.ReadAtUtc.Should().Be(firstReadAt);
    }
}
