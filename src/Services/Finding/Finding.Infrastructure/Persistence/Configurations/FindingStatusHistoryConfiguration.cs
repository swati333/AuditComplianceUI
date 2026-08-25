using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finding.Infrastructure.Persistence.Configurations;

public sealed class FindingStatusHistoryConfiguration : IEntityTypeConfiguration<FindingStatusHistory>
{
    public void Configure(EntityTypeBuilder<FindingStatusHistory> builder)
    {
        builder.ToTable("FindingStatusHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(h => h.ChangedBy).HasMaxLength(200).IsRequired();

        builder.HasIndex(h => h.FindingId);
    }
}
