using NeuroViva.Application.Features.Users.Dtos;

namespace NeuroViva.Application.Features.Users.Queries;

public interface IUserReadRepository
{
    Task<CurrentUserDto?> GetCurrentUserDtoAsync(Guid userId, CancellationToken ct = default);
}
