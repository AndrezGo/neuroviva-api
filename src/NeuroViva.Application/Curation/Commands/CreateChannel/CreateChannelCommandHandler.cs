using MediatR;
using NeuroViva.Application.Common.Models;
using NeuroViva.Domain.Abstractions;
using NeuroViva.Domain.Content;
using NeuroViva.Domain.Content.Repositories;

namespace NeuroViva.Application.Curation.Commands.CreateChannel;

public sealed class CreateChannelCommandHandler
    : IRequestHandler<CreateChannelCommand, Result<CreateChannelResult>>
{
    private readonly IChannelRepository _channelRepo;
    private readonly IUnitOfWork _uow;

    public CreateChannelCommandHandler(
        IChannelRepository channelRepo,
        IUnitOfWork uow)
    {
        _channelRepo = channelRepo;
        _uow = uow;
    }

    public async Task<Result<CreateChannelResult>> Handle(
        CreateChannelCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Error.Validation("channel.name_required", "Name is required.");

        var channel = Channel.Create(request.Name.Trim(), request.Description, request.AvatarUrl);

        await _channelRepo.AddAsync(channel, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new CreateChannelResult(channel.Id);
    }
}
