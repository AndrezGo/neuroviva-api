namespace NeuroViva.Domain.Community.Repositories;

public interface IGroupMemberRepository
{
    Task<GroupMember?> GetAsync(Guid groupId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<GroupMember>> ListActiveByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> IsActiveMemberAsync(Guid groupId, Guid userId, CancellationToken ct = default);
    Task AddAsync(GroupMember member, CancellationToken ct = default);
    void Update(GroupMember member);
}
