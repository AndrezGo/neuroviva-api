namespace NeuroViva.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
}
