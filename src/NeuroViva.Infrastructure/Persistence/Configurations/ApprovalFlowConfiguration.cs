using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NeuroViva.Domain.Content;

namespace NeuroViva.Infrastructure.Persistence.Configurations;

public sealed class ApprovalFlowConfiguration : IEntityTypeConfiguration<ApprovalFlow>
{
    public void Configure(EntityTypeBuilder<ApprovalFlow> builder)
    {
        builder.ToTable("approval_flow");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.ResourceId).HasColumnName("resource_id");
        builder.Property(a => a.Stage).HasColumnName("stage");
        builder.Property(a => a.Status).HasColumnName("status");
        builder.Property(a => a.ReviewedBy).HasColumnName("reviewed_by");
        builder.Property(a => a.Comment).HasColumnName("comment");
        builder.Property(a => a.ReviewedAt).HasColumnName("reviewed_at");
    }
}
