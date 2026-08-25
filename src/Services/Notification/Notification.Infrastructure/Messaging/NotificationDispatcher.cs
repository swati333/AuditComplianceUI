using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Application.Common;
using Notification.Domain.Enums;
using Notification.Infrastructure.Persistence;

namespace Notification.Infrastructure.Messaging;

/// <summary>
/// Delivers Email-channel notifications: polls for rows that are Pending
/// (never attempted) or Failed with a due <c>NextRetryAtUtc</c>, calls
/// <see cref="IEmailProvider"/>, and records the outcome — success marks
/// Sent, failure records the error and either schedules the next
/// exponential-backoff retry or, past the retry budget, dead-letters the
/// row (CLAUDE.md §8's "Delivery status and retry count" made concrete).
/// InApp notifications never appear here — they're marked Sent at creation
/// (see Notification.Create), since there's nothing external to deliver.
/// Runs on its own DI scope per poll since NotificationDbContext is scoped
/// but this service is a singleton — same shape as Audit/Finding Service's
/// OutboxProcessor, applied to delivery instead of publishing.
/// </summary>
public sealed class NotificationDispatcher : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(IServiceScopeFactory scopeFactory, ILogger<NotificationDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingEmailsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Unhandled error while dispatching notifications.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown.
            }
        }
    }

    private async Task DispatchPendingEmailsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var emailProvider = scope.ServiceProvider.GetRequiredService<IEmailProvider>();

        var now = DateTime.UtcNow;
        var due = await dbContext.Notifications
            .Where(n => n.Channel == NotificationChannel.Email)
            .Where(n =>
                n.Status == NotificationStatus.Pending ||
                (n.Status == NotificationStatus.Failed && n.NextRetryAtUtc != null && n.NextRetryAtUtc <= now))
            .OrderBy(n => n.CreatedDate)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (due.Count == 0)
        {
            return;
        }

        foreach (var notification in due)
        {
            try
            {
                await emailProvider.SendAsync(new EmailMessage(notification.RecipientUserId!, notification.Subject, notification.Body), cancellationToken);
                notification.MarkSent("system");
            }
            catch (Exception ex)
            {
                notification.RecordDeliveryFailure(ex.Message, "system");
                _logger.LogWarning(
                    ex,
                    "Failed to deliver notification {NotificationId}, attempt {RetryCount}, status now {Status}.",
                    notification.Id,
                    notification.RetryCount,
                    notification.Status);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
