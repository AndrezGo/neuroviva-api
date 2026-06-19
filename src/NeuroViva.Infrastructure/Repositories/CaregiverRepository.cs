using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Users;
using NeuroViva.Domain.Users.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class CaregiverRepository : ICaregiverRepository
{
    private readonly NeuroVivaDbContext _db;

    public CaregiverRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Caregiver?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.Caregivers.FirstOrDefaultAsync(c => c.UserId == userId, ct);

    public async Task AddAsync(Caregiver caregiver, CancellationToken ct = default)
        => await _db.Caregivers.AddAsync(caregiver, ct);

    public void Update(Caregiver caregiver) => _db.Caregivers.Update(caregiver);
}
