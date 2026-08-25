using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain.Entities;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.UserId).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(p => p.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(p => p.ModifiedBy).HasMaxLength(200);
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasIndex(p => new { p.UserId, p.Channel }).IsUnique();
    }
}
