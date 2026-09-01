using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class FindingReferenceConfiguration : IEntityTypeConfiguration<FindingReference>
{
    public void Configure(EntityTypeBuilder<FindingReference> builder)
    {
        builder.ToTable("FindingReferences");
        builder.HasKey(r => r.FindingId);
        builder.Property(r => r.FindingId).ValueGeneratedNever();

        builder.Property(r => r.AuditId).IsRequired();
        builder.Property(r => r.Severity).HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();

        builder.HasIndex(r => r.AuditId);
    }
}
