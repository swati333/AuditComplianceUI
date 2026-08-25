using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        // Dedupe key is (Id, ConsumerName) — same rationale as Finding Service's identical configuration.
        builder.HasKey(m => new { m.Id, m.ConsumerName });

        builder.Property(m => m.ConsumerName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
        builder.Property(m => m.ProcessedOnUtc).IsRequired();
    }
}
