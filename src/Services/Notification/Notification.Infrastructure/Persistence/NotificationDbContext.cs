using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Entities;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Owns NotificationDb exclusively (CLAUDE.md §5). No <c>OutboxMessages</c>
/// table here — unlike Audit/Finding, Notification Service doesn't publish
/// integration events to other services (there's no "Publish:" list for this
/// phase); its outward-facing side effect is email/in-app delivery, tracked
/// on the Notification entity itself, not the Outbox pattern.
/// <see cref="InboxMessages"/> is still needed — this service is a consumer
/// of 8 event types and must dedupe redelivery (CLAUDE.md §8).
/// </summary>
public sealed class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();

    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
