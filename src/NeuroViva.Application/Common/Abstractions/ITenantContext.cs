namespace NeuroViva.Application.Common.Abstractions;

public interface ITenantContext
{
    Guid? TenantId { get; }
    bool HasTenant => TenantId.HasValue;
}
