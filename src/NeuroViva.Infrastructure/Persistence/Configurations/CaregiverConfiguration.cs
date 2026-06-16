using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Users;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class CaregiverConfiguration : IEntityTypeConfiguration<Caregiver>
{
    public void Configure(EntityTypeBuilder<Caregiver> builder)
    {
        builder.ToTable("caregiver");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.UserId).HasColumnName("user_id");
        builder.Property(c => c.PatientRelationship).HasColumnName("patient_relationship");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.HasIndex(c => c.UserId).IsUnique();
    }
}
