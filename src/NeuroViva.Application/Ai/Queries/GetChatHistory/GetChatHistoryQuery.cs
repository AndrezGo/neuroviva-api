using MediatR;
using NeuroViva.Application.Common.Models;

namespace NeuroViva.Application.Ai.Queries.GetChatHistory;

public sealed record GetChatHistoryQuery(Guid PatientId) : IRequest<Result<IReadOnlyList<ChatMessageDto>>>;
