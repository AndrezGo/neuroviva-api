using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ClinicalRecordAttachmentConfiguration : IEntityTypeConfiguration<ClinicalRecordAttachment>
{
    public void Configure(EntityTypeBuilder<ClinicalRecordAttachment> builder)
    {
        builder.ToTable("clinical_record_attachment");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ClinicalRecordId).HasColumnName("clinical_record_id");
        builder.Property(a => a.StoragePath).HasColumnName("storage_path").HasMaxLength(1024).IsRequired();
        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(512).IsRequired();
        builder.Property(a => a.ContentType).HasColumnName("content_type").HasMaxLength(256).IsRequired();
        builder.Property(a => a.FileSizeBytes).HasColumnName("file_size_bytes").IsRequired(false);
        builder.Property(a => a.UploadedBy).HasColumnName("uploaded_by");
        builder.Property(a => a.UploadedAt).HasColumnName("uploaded_at");
        builder.Property(a => a.ExtractedText)
            .HasColumnName("extracted_text")
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasIndex(a => a.ClinicalRecordId);
    }
}
