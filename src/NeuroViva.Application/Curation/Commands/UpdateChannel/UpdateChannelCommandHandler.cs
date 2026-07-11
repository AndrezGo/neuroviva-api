using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Commands.UpdateChannel;

public sealed class UpdateChannelCommandHandler
    : IRequestHandler<UpdateChannelCommand, Result>
{
    private readonly IChannelRepository _channelRepo;
    private readonly IUnitOfWork _uow;

    public UpdateChannelCommandHandler(
        IChannelRepository channelRepo,
        IUnitOfWork uow)
    {
        _channelRepo = channelRepo;
        _uow = uow;
    }

    public async Task<Result> Handle(
        UpdateChannelCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepo.GetByIdAsync(request.Id, cancellationToken);
        if (channel is null)
            return Error.NotFound("channel.not_found", "Channel not found");

        if (string.IsNullOrWhiteSpace(request.Name))
            return Error.Validation("channel.name_required", "Name is required.");

        channel.Update(request.Name.Trim(), request.Description, request.AvatarUrl);
        _channelRepo.Update(channel);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Ok;
    }
}
