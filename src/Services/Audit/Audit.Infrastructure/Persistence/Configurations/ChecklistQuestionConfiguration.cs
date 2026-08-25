using Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Infrastructure.Persistence.Configurations;

public sealed class ChecklistQuestionConfiguration : IEntityTypeConfiguration<ChecklistQuestion>
{
    public void Configure(EntityTypeBuilder<ChecklistQuestion> builder)
    {
        builder.ToTable("ChecklistQuestions");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedNever();

        builder.Property(q => q.Text).HasMaxLength(1000).IsRequired();

        builder.Property(q => q.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(q => q.ModifiedBy).HasMaxLength(200);
        builder.Property(q => q.RowVersion).IsRowVersion();

        builder.HasIndex(q => new { q.ChecklistId, q.DisplayOrder });
    }
}
