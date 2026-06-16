using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("doctor");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.UserId).HasColumnName("user_id");
        builder.Property(d => d.Specialty).HasColumnName("specialty");
        builder.Property(d => d.MedicalLicense).HasColumnName("medical_license");
        builder.Property(d => d.IsScientificCommittee).HasColumnName("is_scientific_committee");
        builder.Property(d => d.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(d => d.UserId).IsUnique();
        builder.HasIndex(d => d.MedicalLicense).IsUnique();
    }
}
