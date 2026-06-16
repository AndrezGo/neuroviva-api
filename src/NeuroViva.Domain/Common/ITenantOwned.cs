namespace NeuroViva.Domain.Common;

public interface ITenantOwned
{
    Guid TenantId { get; }
}
