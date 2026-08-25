using Finding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finding.Infrastructure.Persistence.Configurations;

public sealed class FindingDocumentConfiguration : IEntityTypeConfiguration<FindingDocument>
{
    public void Configure(EntityTypeBuilder<FindingDocument> builder)
    {
        builder.ToTable("FindingDocuments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.FileName).HasMaxLength(260).IsRequired();
        builder.Property(d => d.BlobReference).HasMaxLength(1000).IsRequired();
        builder.Property(d => d.ContentType).HasMaxLength(200).IsRequired();

        builder.Property(d => d.CreatedBy).HasMaxLength(200).IsRequired();
        builder.Property(d => d.ModifiedBy).HasMaxLength(200);
        builder.Property(d => d.RowVersion).IsRowVersion();

        builder.HasIndex(d => d.FindingId);
    }
}
