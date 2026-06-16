using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Medications;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class MedicationLogConfiguration : IEntityTypeConfiguration<MedicationLog>
{
    public void Configure(EntityTypeBuilder<MedicationLog> builder)
    {
        builder.ToTable("medication_log");
        builder.HasKey(ml => ml.Id);
        builder.Property(ml => ml.Id).HasColumnName("id");
        builder.Property(ml => ml.MedicationId).HasColumnName("medication_id");
        builder.Property(ml => ml.LoggedBy).HasColumnName("logged_by");
        builder.Property(ml => ml.LoggedAt).HasColumnName("logged_at");
        builder.Property(ml => ml.Taken).HasColumnName("taken");
        builder.Property(ml => ml.Notes).HasColumnName("notes");
        builder.Property(ml => ml.CreatedAt).HasColumnName("created_at");
    }
}
