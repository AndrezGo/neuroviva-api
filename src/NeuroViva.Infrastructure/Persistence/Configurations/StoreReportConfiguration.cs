using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Marketplace;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class StoreReportConfiguration : IEntityTypeConfiguration<StoreReport>
{
    public void Configure(EntityTypeBuilder<StoreReport> builder)
    {
        builder.ToTable("store_report");
        builder.HasKey(sr => sr.Id);
        builder.Property(sr => sr.Id).HasColumnName("id");
        builder.Property(sr => sr.StoreId).HasColumnName("store_id");
        builder.Property(sr => sr.ReportedBy).HasColumnName("reported_by");
        builder.Property(sr => sr.Reason).HasColumnName("reason");
        builder.Property(sr => sr.Description).HasColumnName("description");
        builder.Property(sr => sr.Status).HasColumnName("status");
        builder.Property(sr => sr.CreatedAt).HasColumnName("created_at");
    }
}
