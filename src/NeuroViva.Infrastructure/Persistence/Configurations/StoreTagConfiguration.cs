using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Marketplace;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class StoreTagConfiguration : IEntityTypeConfiguration<StoreTag>
{
    public void Configure(EntityTypeBuilder<StoreTag> builder)
    {
        builder.ToTable("store_tag");
        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id).HasColumnName("id");
        builder.Property(st => st.StoreId).HasColumnName("store_id");
        builder.Property(st => st.Tag).HasColumnName("tag");
        builder.HasIndex(st => new { st.StoreId, st.Tag }).IsUnique();
    }
}
