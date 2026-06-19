using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    // The 'patient' table has a CHECK constraint named 'paciente_estado_check' that
    // enforces Spanish status values. Static methods are required because switch and
    // throw expressions are not allowed inside EF Core expression trees.
    private static string ToDbValue(PatientStatus status)
    {
        if (status == PatientStatus.Active)     return "activo";
        if (status == PatientStatus.Inactive)   return "inactivo";
        if (status == PatientStatus.Discharged) return "alta";
        throw new ArgumentOutOfRangeException(nameof(status), status, "Unmapped PatientStatus value.");
    }

    private static PatientStatus FromDbValue(string value)
    {
        if (value == "activo")   return PatientStatus.Active;
        if (value == "inactivo") return PatientStatus.Inactive;
        if (value == "alta")     return PatientStatus.Discharged;
        throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown PatientStatus DB value.");
    }

    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("patient");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.TenantId).HasColumnName("tenant_id");
        builder.Property(p => p.DiseaseId).HasColumnName("disease_id");
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();
        builder.Property(p => p.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(p => p.Status).HasColumnName("status")
            .HasConversion(v => ToDbValue(v), v => FromDbValue(v));
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Ignore(p => p.DomainEvents);
    }
}
