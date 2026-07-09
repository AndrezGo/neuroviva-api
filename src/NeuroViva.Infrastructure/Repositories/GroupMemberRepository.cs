using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class GroupMemberRepository : IGroupMemberRepository
{
    private readonly NeuroVivaDbContext _db;

    public GroupMemberRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<GroupMember?> GetAsync(Guid groupId, Guid userId, CancellationToken ct = default)
        => await _db.GroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId, ct);

    public async Task<IReadOnlyList<GroupMember>> ListActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
        => await _db.GroupMembers
            .AsNoTracking()
            .Where(m => m.UserId == userId && m.Status == "active")
            .ToListAsync(ct);

    public async Task<bool> IsActiveMemberAsync(Guid groupId, Guid userId, CancellationToken ct = default)
        => await _db.GroupMembers
            .AsNoTracking()
            .AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.Status == "active", ct);

    public async Task AddAsync(GroupMember member, CancellationToken ct = default)
        => await _db.GroupMembers.AddAsync(member, ct);

    public void Update(GroupMember member)
        => _db.GroupMembers.Update(member);
}
