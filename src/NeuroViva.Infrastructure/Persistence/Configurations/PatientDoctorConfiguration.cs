using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class PatientDoctorConfiguration : IEntityTypeConfiguration<PatientDoctor>
{
    public void Configure(EntityTypeBuilder<PatientDoctor> builder)
    {
        builder.ToTable("patient_doctor");
        builder.HasKey(pd => pd.Id);
        builder.Property(pd => pd.Id).HasColumnName("id");
        builder.Property(pd => pd.PatientId).HasColumnName("patient_id");
        builder.Property(pd => pd.DoctorId).HasColumnName("doctor_id");
        builder.Property(pd => pd.StartDate).HasColumnName("start_date");
        builder.Property(pd => pd.IsActive).HasColumnName("status")
            .HasConversion(v => v ? "activo" : "inactivo", v => v == "activo");
        builder.HasIndex(pd => new { pd.PatientId, pd.DoctorId }).IsUnique();
    }
}
