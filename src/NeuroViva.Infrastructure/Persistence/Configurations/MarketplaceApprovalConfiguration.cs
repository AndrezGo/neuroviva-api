using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Marketplace;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class MarketplaceApprovalConfiguration : IEntityTypeConfiguration<MarketplaceApproval>
{
    public void Configure(EntityTypeBuilder<MarketplaceApproval> builder)
    {
        builder.ToTable("marketplace_approval");
        builder.HasKey(ma => ma.Id);
        builder.Property(ma => ma.Id).HasColumnName("id");
        builder.Property(ma => ma.StoreId).HasColumnName("store_id");
        builder.Property(ma => ma.Stage).HasColumnName("stage");
        builder.Property(ma => ma.Status).HasColumnName("status");
        builder.Property(ma => ma.ReviewedBy).HasColumnName("reviewed_by");
        builder.Property(ma => ma.Comment).HasColumnName("comment");
        builder.Property(ma => ma.ReviewedAt).HasColumnName("reviewed_at");
    }
}
