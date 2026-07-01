using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class PatientDiseaseConfiguration : IEntityTypeConfiguration<PatientDisease>
{
    public void Configure(EntityTypeBuilder<PatientDisease> builder)
    {
        builder.ToTable("patient_disease");
        builder.HasKey(pd => pd.Id);
        builder.Property(pd => pd.Id).HasColumnName("id");
        builder.Property(pd => pd.PatientId).HasColumnName("patient_id");
        builder.Property(pd => pd.DiseaseId).HasColumnName("disease_id");
        builder.Property(pd => pd.AssignedAt).HasColumnName("assigned_at");
        builder.HasIndex(pd => new { pd.PatientId, pd.DiseaseId }).IsUnique();
    }
}
