using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FindingEntity = Finding.Domain.Entities.Finding;

namespace Finding.Infrastructure.Persistence.Configurations;

public sealed class FindingConfiguration : IEntityTypeConfiguration<FindingEntity>
{
    public void Configure(EntityTypeBuilder<FindingEntity> builder)
    {
        builder.ToTable("Findings");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedNever();

        builder.Property(f => f.AuditId).IsRequired();
        builder.Property(f => f.Title).HasMaxLength(200).IsRequired();
        builder.Property(f => f.Description).HasMaxLength(4000);
        builder.Property(f => f.Severity).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(f => f.RootCauseAnalysis).HasMaxLength(8000);
        builder.Property(f => f.RootCauseAnalysisBy).HasMaxLength(200);

        builder.Property(f => f.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(f => f.ModifiedBy).HasMaxLength(200);

        // Optimistic concurrency (CLAUDE.md §4/§5): SQL Server ROWVERSION.
        builder.Property(f => f.RowVersion).IsRowVersion();

        // Global soft-delete filter (CLAUDE.md §5).
        builder.HasQueryFilter(f => !f.IsDeleted);

        builder.HasMany(f => f.Comments)
            .WithOne()
            .HasForeignKey(c => c.FindingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(f => f.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.Documents)
            .WithOne()
            .HasForeignKey(d => d.FindingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(f => f.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(f => f.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.FindingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(f => f.StatusHistory).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(f => f.AuditId);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.Severity);
        builder.HasIndex(f => f.CreatedDate);
    }
}
