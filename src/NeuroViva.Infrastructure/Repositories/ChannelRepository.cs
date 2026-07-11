using Microsoft.EntityFrameworkCore;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Repositories;
using NeuroViva.Infrastructure.Persistence;

namespace NeuroViva.Infrastructure.Repositories;

public sealed class ChannelRepository : IChannelRepository
{
    private readonly NeuroVivaDbContext _db;

    public ChannelRepository(NeuroVivaDbContext db) => _db = db;

    public async Task<Channel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Channels.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Channel>> ListAllAsync(CancellationToken ct = default)
        => await _db.Channels.ToListAsync(ct);

    public async Task<IReadOnlyList<Channel>> ListByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
        => await _db.Channels.AsNoTracking().Where(c => ids.Contains(c.Id)).ToListAsync(ct);

    public async Task AddAsync(Channel channel, CancellationToken ct = default)
        => await _db.Channels.AddAsync(channel, ct);

    public void Update(Channel channel)
        => _db.Channels.Update(channel);
}
