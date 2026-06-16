using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ResourcePatientConfiguration : IEntityTypeConfiguration<ResourcePatient>
{
    public void Configure(EntityTypeBuilder<ResourcePatient> builder)
    {
        builder.ToTable("resource_patient");
        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id).HasColumnName("id");
        builder.Property(rp => rp.ResourceId).HasColumnName("resource_id");
        builder.Property(rp => rp.PatientId).HasColumnName("patient_id");
        builder.Property(rp => rp.Completed).HasColumnName("completed");
        builder.Property(rp => rp.Progress).HasColumnName("progress");
        builder.Property(rp => rp.AssignedAt).HasColumnName("assigned_at");
        builder.HasIndex(rp => new { rp.ResourceId, rp.PatientId }).IsUnique();
    }
}
