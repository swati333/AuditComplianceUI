using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finding.Infrastructure.Persistence.Configurations;

public sealed class AuditReferenceConfiguration : IEntityTypeConfiguration<AuditReference>
{
    public void Configure(EntityTypeBuilder<AuditReference> builder)
    {
        builder.ToTable("AuditReferences");
        builder.HasKey(r => r.AuditId);
        builder.Property(r => r.AuditId).ValueGeneratedNever();

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.IsClosed).IsRequired();
        builder.Property(r => r.UpdatedAtUtc).IsRequired();
    }
}
