using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Medications;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class MedicationConfiguration : IEntityTypeConfiguration<Medication>
{
    public void Configure(EntityTypeBuilder<Medication> builder)
    {
        builder.ToTable("medication");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.PatientId).HasColumnName("patient_id");
        builder.Property(m => m.Name).HasColumnName("name").IsRequired();
        builder.Property(m => m.Dose).HasColumnName("dose").IsRequired();
        builder.Property(m => m.Frequency).HasColumnName("frequency").IsRequired();
        builder.Property(m => m.PrescribingDoctorName).HasColumnName("prescribing_doctor_name");
        builder.Property(m => m.Notes).HasColumnName("notes");
        builder.Property(m => m.StartDate).HasColumnName("start_date");
        builder.Property(m => m.EndDate).HasColumnName("end_date");
        builder.Property(m => m.IsActive).HasColumnName("active");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Ignore(m => m.DomainEvents);
    }
}
