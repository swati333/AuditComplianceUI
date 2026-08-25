using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ActionPlanEntity = ActionPlan.Domain.Entities.ActionPlan;

namespace ActionPlan.Infrastructure.Persistence.Configurations;

public sealed class ActionPlanConfiguration : IEntityTypeConfiguration<ActionPlanEntity>
{
    public void Configure(EntityTypeBuilder<ActionPlanEntity> builder)
    {
        builder.ToTable("ActionPlans");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.FindingId).IsRequired();
        builder.Property(a => a.ActionType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(4000);
        builder.Property(a => a.OwnerId).HasMaxLength(200).IsRequired();
        builder.Property(a => a.OwnerName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ApproverId).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ApproverName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.DueDate).IsRequired();
        builder.Property(a => a.Priority).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(a => a.RejectionReason).HasMaxLength(2000);

        builder.Property(a => a.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ModifiedBy).HasMaxLength(200);

        // Optimistic concurrency (CLAUDE.md §4/§5): SQL Server ROWVERSION.
        builder.Property(a => a.RowVersion).IsRowVersion();

        // Global soft-delete filter (CLAUDE.md §5).
        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasMany(a => a.Comments)
            .WithOne()
            .HasForeignKey(c => c.ActionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.Evidence)
            .WithOne()
            .HasForeignKey(e => e.ActionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.Evidence).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.ActionPlanId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(a => a.FindingId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Priority);
        builder.HasIndex(a => a.OwnerId);
        builder.HasIndex(a => a.ApproverId);
        builder.HasIndex(a => a.DueDate);
        builder.HasIndex(a => a.CreatedDate);
    }
}
