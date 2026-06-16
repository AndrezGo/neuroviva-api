using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class CaregiverCourseConfiguration : IEntityTypeConfiguration<CaregiverCourse>
{
    public void Configure(EntityTypeBuilder<CaregiverCourse> builder)
    {
        builder.ToTable("caregiver_course");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.DiseaseId).HasColumnName("disease_id");
        builder.Property(c => c.Title).HasColumnName("title");
        builder.Property(c => c.Type).HasColumnName("type");
        builder.Property(c => c.ContentUrl).HasColumnName("content_url");
        builder.Property(c => c.DurationMin).HasColumnName("duration_min");
        builder.Property(c => c.Active).HasColumnName("active");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
    }
}
