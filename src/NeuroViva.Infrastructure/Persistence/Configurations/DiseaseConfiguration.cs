using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Catalog;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class DiseaseConfiguration : IEntityTypeConfiguration<Disease>
{
    public void Configure(EntityTypeBuilder<Disease> builder)
    {
        builder.ToTable("disease");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.Name).HasColumnName("name").IsRequired();
        builder.Property(d => d.Slug).HasColumnName("slug").IsRequired();
        builder.Property(d => d.Description).HasColumnName("description");
        builder.Property(d => d.Category).HasColumnName("category");
        builder.Property(d => d.IsActive).HasColumnName("active");
        builder.HasIndex(d => d.Slug).IsUnique();
    }
}
