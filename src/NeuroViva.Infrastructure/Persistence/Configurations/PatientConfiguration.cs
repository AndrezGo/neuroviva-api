using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Patients;
using NeuroViva.Domain.Patients.Enums;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
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
            .HasConversion(v => v.ToString().ToLowerInvariant(), v => Enum.Parse<PatientStatus>(v, true));
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Ignore(p => p.DomainEvents);
    }
}
