using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditStatusHistoryConfiguration : IEntityTypeConfiguration<AuditStatusHistory>
{
    public void Configure(EntityTypeBuilder<AuditStatusHistory> builder)
    {
        builder.ToTable("AuditStatusHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.ChangedBy).HasMaxLength(200).IsRequired();
        builder.Property(h => h.Reason).HasMaxLength(1000);

        builder.HasIndex(h => h.AuditId);
    }
}
