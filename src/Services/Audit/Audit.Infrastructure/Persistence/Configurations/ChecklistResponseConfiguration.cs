using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class ChecklistResponseConfiguration : IEntityTypeConfiguration<ChecklistResponse>
{
    public void Configure(EntityTypeBuilder<ChecklistResponse> builder)
    {
        builder.ToTable("ChecklistResponses");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.AnswerText).HasMaxLength(4000).IsRequired();

        builder.Property(r => r.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(r => r.ModifiedBy).HasMaxLength(200);
        builder.Property(r => r.RowVersion).IsRowVersion();

        // One response per question per audit — Audit.RecordChecklistResponse
        // updates in place rather than inserting a duplicate.
        builder.HasIndex(r => new { r.AuditId, r.ChecklistQuestionId }).IsUnique();
    }
}
