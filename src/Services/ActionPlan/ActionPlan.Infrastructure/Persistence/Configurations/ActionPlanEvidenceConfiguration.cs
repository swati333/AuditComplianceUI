using ActionPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActionPlan.Infrastructure.Persistence.Configurations;

public sealed class ActionPlanEvidenceConfiguration : IEntityTypeConfiguration<ActionPlanEvidence>
{
    public void Configure(EntityTypeBuilder<ActionPlanEvidence> builder)
    {
        builder.ToTable("ActionPlanEvidence");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.FileName).HasMaxLength(260).IsRequired();
        builder.Property(e => e.BlobReference).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.ContentType).HasMaxLength(200).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(200).IsRequired();

        builder.HasIndex(e => e.ActionPlanId);
    }
}
