using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AuditEntity = Audit.Domain.Entities.Audit;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditConfiguration : IEntityTypeConfiguration<AuditEntity>
{
    public void Configure(EntityTypeBuilder<AuditEntity> builder)
    {
        builder.ToTable("Audits");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(2000);
        builder.Property(a => a.Scope).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.Location).HasMaxLength(300).IsRequired();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.CancellationReason).HasMaxLength(1000);

        builder.Property(a => a.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(a => a.ModifiedBy).HasMaxLength(200);

        // Optimistic concurrency (CLAUDE.md §4/§5): SQL Server ROWVERSION,
        // auto-updated by the engine on every UPDATE to this row.
        builder.Property(a => a.RowVersion).IsRowVersion();

        // Global soft-delete filter (CLAUDE.md §5): soft-deleted audits never
        // appear in any query against this DbSet unless explicitly ignored.
        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasMany(a => a.TeamMembers)
            .WithOne()
            .HasForeignKey(m => m.AuditId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.TeamMembers).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.ChecklistResponses)
            .WithOne()
            .HasForeignKey(r => r.AuditId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.ChecklistResponses).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(a => a.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.AuditId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(a => a.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Location);
        builder.HasIndex(a => a.CreatedDate);
    }
}
