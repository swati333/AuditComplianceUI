using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class OpenRequiredActionConfiguration : IEntityTypeConfiguration<OpenRequiredAction>
{
    public void Configure(EntityTypeBuilder<OpenRequiredAction> builder)
    {
        builder.ToTable("OpenRequiredActions");
        builder.HasKey(a => a.ActionPlanId);
        builder.Property(a => a.ActionPlanId).ValueGeneratedNever();

        builder.Property(a => a.FindingId).IsRequired();
        builder.Property(a => a.AuditId).IsRequired();
        builder.Property(a => a.AssignedAtUtc).IsRequired();

        builder.HasIndex(a => a.AuditId);
    }
}
