using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class AuditTeamMemberConfiguration : IEntityTypeConfiguration<AuditTeamMember>
{
    public void Configure(EntityTypeBuilder<AuditTeamMember> builder)
    {
        builder.ToTable("AuditTeamMembers");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.UserId).HasMaxLength(200).IsRequired();
        builder.Property(m => m.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

        builder.Property(m => m.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(m => m.ModifiedBy).HasMaxLength(200);
        builder.Property(m => m.RowVersion).IsRowVersion();

        // Same (user, role) can't be assigned to the same audit twice —
        // mirrors the in-aggregate check in Audit.AssignTeamMember as a
        // database-level backstop.
        builder.HasIndex(m => new { m.AuditId, m.UserId, m.Role }).IsUnique();
    }
}
