namespace NeuroViva.Domain.Users.Repositories;

public interface ICaregiverRepository
{
    Task<Caregiver?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Caregiver caregiver, CancellationToken ct = default);
    void Update(Caregiver caregiver);
}
