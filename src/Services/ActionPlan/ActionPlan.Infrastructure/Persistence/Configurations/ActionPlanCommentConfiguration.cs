using ActionPlan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ActionPlan.Infrastructure.Persistence.Configurations;

public sealed class ActionPlanCommentConfiguration : IEntityTypeConfiguration<ActionPlanComment>
{
    public void Configure(EntityTypeBuilder<ActionPlanComment> builder)
    {
        builder.ToTable("ActionPlanComments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.AuthorId).HasMaxLength(200).IsRequired();
        builder.Property(c => c.AuthorName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Text).HasMaxLength(4000).IsRequired();
        builder.Property(c => c.CreatedBy).HasMaxLength(200).IsRequired();

        builder.HasIndex(c => c.ActionPlanId);
    }
}
