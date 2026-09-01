using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finding.Infrastructure.Persistence.Configurations;

public sealed class ActionPlanReferenceConfiguration : IEntityTypeConfiguration<ActionPlanReference>
{
    public void Configure(EntityTypeBuilder<ActionPlanReference> builder)
    {
        builder.ToTable("ActionPlanReferences");
        builder.HasKey(r => r.FindingId);
        builder.Property(r => r.FindingId).ValueGeneratedNever();

        builder.Property(r => r.FirstActionPlanId).IsRequired();
        builder.Property(r => r.CreatedAtUtc).IsRequired();
    }
}
