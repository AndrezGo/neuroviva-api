using MediatR;
using NeuroViva.Application.Ai.Queries;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Ai.Commands.SendChatMessage;

public sealed record SendChatMessageCommand(Guid PatientId, string Message)
    : IRequest<Result<ChatMessageDto>>;
