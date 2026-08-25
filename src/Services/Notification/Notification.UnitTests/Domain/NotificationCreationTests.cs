using FluentAssertions;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.UnitTests.Domain;

public class NotificationCreationTests
{
    private const string Actor = "system";

    [Fact]
    public void Create_marks_InApp_notifications_Sent_immediately()
    {
        var notification = NotificationEntity.Create("user-1", NotificationChannel.InApp, "Code", "Subject", "Body", Guid.NewGuid(), "SomeEvent", Actor);

        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void Create_leaves_Email_notifications_Pending()
    {
        var notification = NotificationEntity.Create("user-1", NotificationChannel.Email, "Code", "Subject", "Body", Guid.NewGuid(), "SomeEvent", Actor);

        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.SentAtUtc.Should().BeNull();
    }

    [Fact]
    public void Create_allows_a_null_recipient_for_broadcast_notifications()
    {
        var notification = NotificationEntity.Create(null, NotificationChannel.InApp, "Code", "Subject", "Body", Guid.NewGuid(), "SomeEvent", Actor);

        notification.RecipientUserId.Should().BeNull();
    }

    [Fact]
    public void Create_stamps_CorrelationId_and_SourceEventType_for_traceability()
    {
        var correlationId = Guid.NewGuid();

        var notification = NotificationEntity.Create("user-1", NotificationChannel.Email, "Code", "Subject", "Body", correlationId, "CriticalFindingCreated", Actor);

        notification.CorrelationId.Should().Be(correlationId);
        notification.SourceEventType.Should().Be("CriticalFindingCreated");
    }
}
