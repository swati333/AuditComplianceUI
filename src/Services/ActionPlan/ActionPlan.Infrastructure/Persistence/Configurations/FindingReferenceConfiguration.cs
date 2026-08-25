using ActionPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActionPlan.Infrastructure.Persistence.Configurations;

public sealed class FindingReferenceConfiguration : IEntityTypeConfiguration<FindingReference>
{
    public void Configure(EntityTypeBuilder<FindingReference> builder)
    {
        builder.ToTable("FindingReferences");
        builder.HasKey(r => r.FindingId);
        builder.Property(r => r.FindingId).ValueGeneratedNever();

        builder.Property(r => r.Title).HasMaxLength(200).IsRequired();
        builder.Property(r => r.IsClosed).IsRequired();
        builder.Property(r => r.UpdatedAtUtc).IsRequired();
    }
}
