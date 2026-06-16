using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class PatientCaregiverConfiguration : IEntityTypeConfiguration<PatientCaregiver>
{
    public void Configure(EntityTypeBuilder<PatientCaregiver> builder)
    {
        builder.ToTable("patient_caregiver");
        builder.HasKey(pc => pc.Id);
        builder.Property(pc => pc.Id).HasColumnName("id");
        builder.Property(pc => pc.PatientId).HasColumnName("patient_id");
        builder.Property(pc => pc.CaregiverId).HasColumnName("caregiver_id");
        builder.Property(pc => pc.CareRole).HasColumnName("care_role");
        builder.Property(pc => pc.StartDate).HasColumnName("start_date");
        builder.HasIndex(pc => new { pc.PatientId, pc.CaregiverId }).IsUnique();
    }
}
