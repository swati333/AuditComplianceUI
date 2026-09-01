using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class OpenCriticalFindingConfiguration : IEntityTypeConfiguration<OpenCriticalFinding>
{
    public void Configure(EntityTypeBuilder<OpenCriticalFinding> builder)
    {
        builder.ToTable("OpenCriticalFindings");
        builder.HasKey(f => f.FindingId);
        builder.Property(f => f.FindingId).ValueGeneratedNever();

        builder.Property(f => f.AuditId).IsRequired();
        builder.Property(f => f.DetectedAtUtc).IsRequired();

        builder.HasIndex(f => f.AuditId);
    }
}
