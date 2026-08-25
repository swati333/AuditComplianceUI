using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finding.Infrastructure.Persistence.Configurations;

public sealed class FindingCommentConfiguration : IEntityTypeConfiguration<FindingComment>
{
    public void Configure(EntityTypeBuilder<FindingComment> builder)
    {
        builder.ToTable("FindingComments");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.AuthorId).HasMaxLength(200).IsRequired();
        builder.Property(c => c.AuthorName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Text).HasMaxLength(4000).IsRequired();

        builder.Property(c => c.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ModifiedBy).HasMaxLength(200);
        builder.Property(c => c.RowVersion).IsRowVersion();

        builder.HasIndex(c => c.FindingId);
    }
}
