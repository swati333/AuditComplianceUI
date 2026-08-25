using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class ChecklistConfiguration : IEntityTypeConfiguration<Checklist>
{
    public void Configure(EntityTypeBuilder<Checklist> builder)
    {
        builder.ToTable("Checklists");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(2000);

        builder.Property(c => c.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ModifiedBy).HasMaxLength(200);
        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasMany(c => c.Questions)
            .WithOne()
            .HasForeignKey(q => q.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Questions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
