using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Code).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(t => t.SubjectTemplate).HasMaxLength(500);
        builder.Property(t => t.BodyTemplate).HasMaxLength(4000).IsRequired();

        builder.Property(t => t.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(t => t.ModifiedBy).HasMaxLength(200);
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasQueryFilter(t => !t.IsDeleted);

        // One template per (event code, channel).
        builder.HasIndex(t => new { t.Code, t.Channel }).IsUnique();
    }
}
