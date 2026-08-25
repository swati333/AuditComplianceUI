using Ehs.Contracts.Events;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.EventHandlers;
using Notification.Contracts.ExternalEvents;
using Notification.Domain.Enums;
using Notification.Infrastructure.Messaging;
using Notification.Infrastructure.Persistence;

namespace Notification.IntegrationTests;

/// <summary>
/// Proves the background NotificationDispatcher actually delivers Email
/// notifications end-to-end: creates one via ActionPlanAssigned (which
/// addresses a real recipient, so an Email template applies), then polls
/// the database until the dispatcher has picked it up, called the fake
/// local email provider, and marked it Sent — with a real .eml file on disk
/// to show for it.
/// </summary>
public sealed class NotificationDispatcherApiTests : IntegrationTestBase
{
    public NotificationDispatcherApiTests(NotificationApiFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task An_Email_notification_is_picked_up_and_marked_Sent_by_the_background_dispatcher()
    {
        using var scope = Factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IntegrationEventConsumer>();
        var handler = scope.ServiceProvider.GetRequiredService<ActionPlanAssignedHandler>();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

        var assignee = $"dispatch-test-{Guid.NewGuid()}";
        var envelope = EventEnvelope.Create(
            new ActionPlanAssigned(Guid.NewGuid(), Guid.NewGuid(), assignee, "Fix the exit signage", DateTime.UtcNow.AddDays(7), DateTime.UtcNow),
            "ActionPlanService",
            Guid.NewGuid());
        await consumer.ConsumeAsync(envelope, "Notification.ActionPlanAssignedHandler", handler);

        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            var email = await dbContext.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.RecipientUserId == assignee && n.Channel == NotificationChannel.Email);

            if (email is { Status: NotificationStatus.Sent })
            {
                email.SentAtUtc.Should().NotBeNull();
                Directory.GetFiles(Factory.FakeEmailOutputDirectory, "*.eml").Should().NotBeEmpty();
                return;
            }

            await Task.Delay(500);
        }

        Assert.Fail("Dispatcher did not mark the Email notification Sent within 30 seconds.");
    }
}
