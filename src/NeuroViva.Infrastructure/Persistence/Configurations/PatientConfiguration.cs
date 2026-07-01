using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;
using NeuroViva.Domain.Users;

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
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();
        builder.Property(p => p.DocumentNumber)
            .HasColumnName("document_number")
            .IsRequired()
            .HasMaxLength(30);
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.DateOfBirth).HasColumnName("date_of_birth");
        builder.Property(p => p.Status).HasColumnName("status")
            .HasConversion(v => ToDbValue(v), v => FromDbValue(v));
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Ignore(p => p.DomainEvents);

        // Diseases is in-memory domain bookkeeping only (mutated via Patient.SetDiseases).
        // Persistence always goes through IPatientDiseaseRepository, never through this
        // navigation — ignoring it here prevents EF's change tracker from independently
        // syncing PatientDisease rows and racing with the repository's own delete/insert,
        // which was causing DbUpdateConcurrencyException ("0 rows affected") on updates.
        builder.Ignore(p => p.Diseases);

        // Unique composite index: (tenant_id, document_number)
        // Domain always normalises document_number to UPPER, matching the DB-level UPPER() functional index.
        builder.HasIndex(p => new { p.TenantId, p.DocumentNumber })
            .IsUnique()
            .HasDatabaseName("uq_patient_tenant_document");

        // Unique partial index on user_id: only enforced when user_id IS NOT NULL
        builder.HasIndex(p => p.UserId)
            .IsUnique()
            .HasFilter("user_id IS NOT NULL")
            .HasDatabaseName("uq_patient_user_id_partial");

        // FK to User: optional, set null on user deletion (matches the DBA's SQL constraint).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
