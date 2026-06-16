using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class CourseProgressConfiguration : IEntityTypeConfiguration<CourseProgress>
{
    public void Configure(EntityTypeBuilder<CourseProgress> builder)
    {
        builder.ToTable("course_progress");
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id).HasColumnName("id");
        builder.Property(cp => cp.CourseId).HasColumnName("course_id");
        builder.Property(cp => cp.CaregiverId).HasColumnName("caregiver_id");
        builder.Property(cp => cp.Percentage).HasColumnName("percentage");
        builder.Property(cp => cp.Completed).HasColumnName("completed");
        builder.Property(cp => cp.LastActivityAt).HasColumnName("last_activity_at");
        builder.HasIndex(cp => new { cp.CourseId, cp.CaregiverId }).IsUnique();
    }
}
