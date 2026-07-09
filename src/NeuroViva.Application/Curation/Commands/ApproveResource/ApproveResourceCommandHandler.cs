using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Commands.ApproveResource;

public sealed class ApproveResourceCommandHandler
    : IRequestHandler<ApproveResourceCommand, Result>
{
    private readonly IResourceRepository _resourceRepo;
    private readonly IUnitOfWork _uow;

    public ApproveResourceCommandHandler(
        IResourceRepository resourceRepo,
        IUnitOfWork uow)
    {
        _resourceRepo = resourceRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        ApproveResourceCommand request,
        CancellationToken cancellationToken)
    {
        var resource = await _resourceRepo.GetByIdAsync(request.ResourceId, cancellationToken);
        if (resource is null)
            return Error.NotFound("resource.not_found", "Resource not found");

        resource.Approve();
        _resourceRepo.Update(resource);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
