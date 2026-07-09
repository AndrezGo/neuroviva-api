using MediatR;
using NeuroViva.Application.Common.Abstractions;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Community;
using NeuroViva.Domain.Community.Repositories;

namespace NeuroViva.Application.Community.Commands.CreateGroup;

public sealed class CreateGroupCommandHandler
    : IRequestHandler<CreateGroupCommand, Result<CreateGroupResult>>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IGroupRepository _groupRepo;
    private readonly IUnitOfWork _uow;

    public CreateGroupCommandHandler(
        ICurrentUserService currentUser,
        IGroupRepository groupRepo,
        IUnitOfWork uow)
    {
        _currentUser = currentUser;
        _groupRepo = groupRepo;
        _uow = uow;
    }

    public async Task<Result<CreateGroupResult>> Handle(
        CreateGroupCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Error.Unauthorized("User not synced. Call /users/sync first.");

        var userId = _currentUser.UserId.Value;

        var existing = await _groupRepo.GetBySlugAsync(request.Slug, cancellationToken);
        if (existing is not null)
            return Error.Conflict("group.slug_exists", $"A group with slug '{request.Slug}' already exists.");

        var group = Group.Create(
            creatorId: userId,
            name: request.Name,
            slug: request.Slug,
            visibility: request.Visibility,
            diseaseId: request.DiseaseId);

        await _groupRepo.AddAsync(group, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateGroupResult(group.Id);
    }
}
