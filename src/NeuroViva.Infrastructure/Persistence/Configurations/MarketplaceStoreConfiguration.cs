using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Marketplace;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class MarketplaceStoreConfiguration : IEntityTypeConfiguration<MarketplaceStore>
{
    public void Configure(EntityTypeBuilder<MarketplaceStore> builder)
    {
        builder.ToTable("marketplace_store");
        builder.HasKey(ms => ms.Id);
        builder.Property(ms => ms.Id).HasColumnName("id");
        builder.Property(ms => ms.OwnerId).HasColumnName("owner_id");
        builder.Property(ms => ms.DiseaseId).HasColumnName("disease_id");
        builder.Property(ms => ms.Name).HasColumnName("name");
        builder.Property(ms => ms.Description).HasColumnName("description");
        builder.Property(ms => ms.StoreUrl).HasColumnName("store_url");
        builder.Property(ms => ms.LogoUrl).HasColumnName("logo_url");
        builder.Property(ms => ms.Category).HasColumnName("category");
        builder.Property(ms => ms.ApprovalStatus).HasColumnName("approval_status");
        builder.Property(ms => ms.Active).HasColumnName("active");
        builder.Property(ms => ms.CreatedAt).HasColumnName("created_at");
        builder.Ignore(ms => ms.DomainEvents);
    }
}
