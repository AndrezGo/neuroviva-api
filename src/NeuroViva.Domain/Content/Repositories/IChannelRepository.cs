namespace NeuroViva.Domain.Content.Repositories;

public interface IChannelRepository
{
    Task<Channel?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Channel>> ListAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Channel>> ListByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task AddAsync(Channel channel, CancellationToken ct = default);
    void Update(Channel channel);
}
