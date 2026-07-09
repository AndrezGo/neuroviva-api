using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Commands.RejectResource;

public sealed class RejectResourceCommandHandler
    : IRequestHandler<RejectResourceCommand, Result>
{
    private readonly IResourceRepository _resourceRepo;
    private readonly IUnitOfWork _uow;

    public RejectResourceCommandHandler(
        IResourceRepository resourceRepo,
        IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        RejectResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceRepo.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Error.NotFound("resource.not_found", "Resource not found");

        resource.Reject();
        _resourceRepo.Update(resource);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
