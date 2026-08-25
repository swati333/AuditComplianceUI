using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();

        builder.Property(n => n.RecipientUserId).HasMaxLength(200);
        builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.TemplateCode).HasMaxLength(100).IsRequired();
        builder.Property(n => n.Subject).HasMaxLength(500).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(8000).IsRequired();
        builder.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(n => n.LastError).HasMaxLength(2000);
        builder.Property(n => n.SourceEventType).HasMaxLength(100).IsRequired();

        builder.Property(n => n.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(n => n.ModifiedBy).HasMaxLength(200);
        builder.Property(n => n.RowVersion).IsRowVersion();

        builder.HasQueryFilter(n => !n.IsDeleted);

        builder.HasIndex(n => n.RecipientUserId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.Channel);
        builder.HasIndex(n => n.CreatedDate);

        // The dispatcher polls WHERE Channel = Email AND Status IN (Pending, Failed) AND (NextRetryAtUtc IS NULL OR NextRetryAtUtc <= now).
        builder.HasIndex(n => new { n.Channel, n.Status, n.NextRetryAtUtc });
    }
}
