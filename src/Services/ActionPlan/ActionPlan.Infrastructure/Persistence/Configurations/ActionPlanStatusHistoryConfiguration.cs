using ActionPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActionPlan.Infrastructure.Persistence.Configurations;

public sealed class ActionPlanStatusHistoryConfiguration : IEntityTypeConfiguration<ActionPlanStatusHistory>
{
    public void Configure(EntityTypeBuilder<ActionPlanStatusHistory> builder)
    {
        builder.ToTable("ActionPlanStatusHistories");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.FromStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(h => h.ToStatus).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(h => h.ChangedBy).HasMaxLength(200).IsRequired();
        builder.Property(h => h.Reason).HasMaxLength(2000);

        builder.HasIndex(h => h.ActionPlanId);
    }
}
