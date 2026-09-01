using Ehs.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");

        // Dedupe key is (Id, ConsumerName) per Ehs.SharedKernel.Messaging.InboxMessage's
        // doc comment: the same event redelivered to the same handler must be a
        // no-op, but distinct handlers each get their own row for the same event.
        builder.HasKey(m => new { m.Id, m.ConsumerName });

        builder.Property(m => m.ConsumerName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Type).HasMaxLength(200).IsRequired();
        builder.Property(m => m.ProcessedOnUtc).IsRequired();
    }
}
